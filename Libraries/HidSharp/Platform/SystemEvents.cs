#region License
/* Copyright 2017 James F. Bellinger <http://www.zer7.com/software/hidsharp>

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

      http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing,
   software distributed under the License is distributed on an
   "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
   KIND, either express or implied.  See the License for the
   specific language governing permissions and limitations
   under the License. */
#endregion

// Last updated 2017/12/9.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace HidSharp.Platform.SystemEvents
{
    #region Native
    #region POSIX (Shared)
    abstract class PosixNativeMethods
    {
        public static readonly IntPtr IntPtrNegativeOne = (IntPtr.Size == 8) ? new IntPtr((long)(-1)) : new IntPtr((int)(-1));

        public abstract int GetTickCount();

        public abstract int shm_open(string filename, int oflag, int mode);
        public abstract int chmod(string filename, int mode);
        public abstract int fchmod(int filedes, int mode);
        public abstract int ftruncate(int filedes, long length);
        public abstract int close(int filedes);

        public abstract IntPtr mmap(IntPtr addr, UIntPtr size, int prot, int flags, int fd, long offset);
        public abstract int munmap(IntPtr addr, UIntPtr length);

        public int GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }

        public int retry(Func<int> sysfunc)
        {
            while (true)
            {
                var ret = sysfunc();
                if (ret != -1 || GetLastError() != EINTR) { return ret; }
            }
        }

        public IntPtr retry(Func<IntPtr> sysfunc)
        {
            while (true)
            {
                var ret = sysfunc();
                if (ret != IntPtrNegativeOne || GetLastError() != EINTR) { return ret; }
            }
        }

        public abstract int MAP_SHARED { get; }
        public abstract int O_RDWR { get; }
        public abstract int O_CREAT { get; }
        public abstract int PROT_READ { get; }
        public abstract int PROT_WRITE { get; }
        public abstract int EINTR { get; }
    }
    #endregion
    #endregion

    #region System
    internal abstract class SystemEvent : IDisposable
    {
        protected SystemEvent(string name)
        {
            if (name == null) { throw new ArgumentNullException(); }
            Name = name;
        }

        public abstract void Dispose();
        public abstract void Reset();
        public abstract void Set();

        public bool Wait(int timeout)
        {
            try
            {
                return WaitHandle.WaitOne(timeout);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }

        public abstract bool CreatedNew { get; }
        public string Name { get; private set; }
        public abstract WaitHandle WaitHandle { get; }
    }

    internal abstract class SystemMutex : IDisposable
    {
        static HashSet<string> _antirecursionList = new HashSet<string>();
        Thread _lockThread; // Mostly for debugging. Mutexes must be released by the threads that locked them.

        protected SystemMutex(string name)
        {
            if (name == null) { throw new ArgumentNullException(); }
            Name = name;
        }

        public abstract void Dispose();
        protected abstract bool WaitOne(int timeout);
        protected abstract void ReleaseMutex();

        sealed class ResourceLock : IDisposable
        {
            int _disposed;

            internal SystemMutex M;

            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0) { return; }

                try
                {
                    M.ReleaseMutexOuter();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public bool TryLock(out IDisposable @lock)
        {
            return TryLock(Timeout.Infinite, out @lock);
        }

        public bool TryLock(int timeout, out IDisposable @lock)
        {
            @lock = null;

            try
            {
                if (!WaitOneOuter(timeout)) { return false; }
            }
            catch (AbandonedMutexException e)
            {
                Debug.WriteLine(e);
                return false;
            }

            @lock = new ResourceLock() { M = this };
            return true;
        }

        bool WaitOneOuter(int timeout)
        {
            if (!WaitOneInner(timeout)) { return false; }

            lock (_antirecursionList)
            {
                if (_antirecursionList.Contains(Name))
                {
                    ReleaseMutexInner(); return false;
                }

                _antirecursionList.Add(Name); return true;
            }
        }

        bool WaitOneInner(int timeout)
        {
            try
            {
                if (!WaitOne(timeout))
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e); return false;
            }

            if (_lockThread != null) { throw new InvalidOperationException(); }
            _lockThread = Thread.CurrentThread; return true;
        }

        void ReleaseMutexOuter()
        {
            lock (_antirecursionList)
            {
                _antirecursionList.Remove(Name);
                ReleaseMutexInner();
            }
        }

        void ReleaseMutexInner()
        {
            if (_lockThread != Thread.CurrentThread) { throw new InvalidOperationException(); }
            ReleaseMutex(); _lockThread = null;
        }

        public abstract bool CreatedNew { get; }
        public string Name { get; private set; }
    }

    internal abstract class EventManager
    {
        internal abstract void Start();

        public abstract SystemEvent CreateEvent(string name);
        public abstract SystemMutex CreateMutex(string name);

        public bool MutexMayExist(string name) // Call it "MayExist" because it *might* not -- another could call this at the same time.
        {
            using (var mutex = CreateMutex(name)) { return !mutex.CreatedNew; }
        }
    }
    #endregion

    #region POSIX Implementation (Shared)
    internal abstract class PosixEventManager : EventManager
    {
        const string EventsKind = "Events";
        const string MutexesKind = "Mutexes";
        const int DontRefreshInterval = 4000;
        const int RefreshInterval = 5000;
        const int TimeoutInterval = 30000;
        const int TimeTravelInterval = 1000;

        sealed class PosixEvent : SystemEvent
        {
            struct LockStructure
            {
                public int Ttl;
                public int RefCount;
                public int SetID;
                public int ResetID;
            }

            bool _createdNew;
            ManualResetEvent _event;
            PosixEventManager _manager;
            object _refreshHandle;

            public PosixEvent(PosixEventManager manager, string name)
                : base(name)
            {
                if (manager == null) { throw new ArgumentNullException(); }
                _manager = manager;
                _event = new ManualResetEvent(false);

                UpdateEventStruct(inL =>
                {
                    if (inL.RefCount == 0) { _createdNew = true; }

                    var outL = inL;
                    outL.RefCount++;
                    outL.Ttl = _manager.GetTickCount();
                    return outL;
                });

                manager.RegisterRefreshCallback(Refresh, out _refreshHandle, GetEventFilename(EventsKind, Name), GetSHMFilename(EventsKind, Name));
            }

            public override void Dispose()
            {
                if (_event != null)
                {
                    _manager.UnregisterRefreshCallback(ref _refreshHandle);

                    UpdateEventStruct(inL =>
                    {
                        var outL = inL;
                        outL.RefCount--;
                        return outL;
                    });

                    _event.Close();
                    _event = null;
                }
            }

            void Refresh()
            {
                UpdateEventStruct(inL =>
                {
                    int ttl = _manager.GetTickCount();
                    if ((uint)(ttl - inL.Ttl) < DontRefreshInterval) { return null; }

                    var outL = inL;
                    outL.Ttl = ttl;
                    return outL;
                });
            }

            public override void Reset()
            {
                UpdateEventStruct(inL =>
                {
                    if (inL.ResetID == inL.SetID) { return null; }

                    var outL = inL;
                    outL.ResetID = outL.SetID;
                    outL.Ttl = _manager.GetTickCount();
                    return outL;
                }, true);
            }

            public override void Set()
            {
                UpdateEventStruct(inL =>
                {
                    if (inL.ResetID != inL.SetID) { return null; }

                    var outL = inL;
                    outL.SetID++;
                    outL.Ttl = _manager.GetTickCount();
                    return outL;
                }, true);
            }

            void UpdateEventStream(Func<byte[], bool> editCallback)
            {
                _manager.UpdateEventStream(EventsKind, Name, 16, editCallback);
            }

            void UpdateEventStruct(Func<LockStructure, LockStructure?> editCallback, bool updateFSW = false)
            {
                UpdateEventStream(buffer =>
                {
                    int ttl = _manager.GetTickCount();

                    var inL = new LockStructure();
                    inL.Ttl = BitConverter.ToInt32(buffer, 0);
                    inL.RefCount = BitConverter.ToInt32(buffer, 4);
                    inL.SetID = BitConverter.ToInt32(buffer, 8);
                    inL.ResetID = BitConverter.ToInt32(buffer, 12);

                    if (inL.RefCount <= 0)
                    {
                        inL = new LockStructure();
                    }

                    int refreshTime = ttl - inL.Ttl;
                    if (inL.Ttl != 0)
                    {
                        if (refreshTime >= TimeoutInterval || refreshTime <= -TimeTravelInterval)
                        {
                            Debug.WriteLine(string.Format("{0} : Event Timed Out", Name));
                            inL = new LockStructure();
                        }
                    }

                    var outLmaybe = editCallback(inL);
                    if (outLmaybe == null) { UpdateEvent(inL.SetID != inL.ResetID); return false; }
                    var outL = outLmaybe.Value;

                    Array.Copy(BitConverter.GetBytes(outL.Ttl), 0, buffer, 0, 4);
                    Array.Copy(BitConverter.GetBytes(outL.RefCount), 0, buffer, 4, 4);
                    Array.Copy(BitConverter.GetBytes(outL.SetID), 0, buffer, 8, 4);
                    Array.Copy(BitConverter.GetBytes(outL.ResetID), 0, buffer, 12, 4);
                    UpdateEvent(outL.SetID != inL.ResetID); return updateFSW;
                });
            }

            void UpdateEvent(bool set)
            {
                if (set) { _event.Set(); } else { _event.Reset(); }
            }

            public override bool CreatedNew
            {
                get { return _createdNew; }
            }

            public override WaitHandle WaitHandle
            {
                get { return _event; }
            }
        }

        sealed class PosixMutex : SystemMutex
        {
            struct LockStructure
            {
                public int Ttl;
                public int RefCount;
                //public int Reserved;
                public int LockTtl;
                public Guid LockGuid;
            }

            bool _createdNew;
            Guid _guid;
            PosixEventManager _manager;
            object _refreshHandle;

            public PosixMutex(PosixEventManager manager, string name)
                : base(name)
            {
                if (manager == null) { throw new ArgumentNullException(); }
                _guid = Guid.NewGuid(); // Will not equal Guid.Empty.
                _manager = manager;

                UpdateEventStruct(inL =>
                {
                    if (inL.RefCount == 0) { _createdNew = true; }

                    var outL = inL;
                    outL.RefCount++;
                    return outL;
                });

                _manager.RegisterRefreshCallback(Refresh, out _refreshHandle, null, null);
            }

            public override void Dispose()
            {
                _manager.UnregisterRefreshCallback(ref _refreshHandle);

                UpdateEventStruct(inL =>
                {
                    var outL = inL;
                    if (outL.LockGuid == _guid)
                    {
                        outL.LockGuid = Guid.Empty;
                    }
                    outL.RefCount--;
                    return outL;
                });
            }

            void Refresh()
            {
                UpdateEventStruct(inL =>
                {
                    int ttl = _manager.GetTickCount();

                    var outL = inL;
                    return outL;
                });
            }

            protected override bool WaitOne(int timeout)
            {
                int start = _manager.GetTickCount();

                do
                {
                    bool locked = false;

                    UpdateEventStruct(inL =>
                    {
                        if (inL.LockGuid != Guid.Empty)
                        {
                            if (inL.LockGuid == _guid) { throw new InvalidOperationException("Already locked by this mutex."); }
                            return null;
                        }

                        var outL = inL;
                        outL.LockGuid = _guid; locked = true;
                        return outL;
                    });

                    if (locked)
                    {
                        return true;
                    }
                    Thread.Sleep(50);
                }
                while ((uint)(_manager.GetTickCount() - start) <= (uint)timeout); // This covers Timeout.Infinite as well.

                return false;
            }

            protected override void ReleaseMutex()
            {
                UpdateEventStruct(inL =>
                {
                    if (inL.LockGuid == Guid.Empty) { throw new InvalidOperationException("Not locked by anyone."); }
                    if (inL.LockGuid != _guid) { throw new InvalidOperationException("Not locked by this mutex."); }

                    var outL = inL;
                    outL.LockGuid = Guid.Empty;
                    return outL;
                });
            }

            void UpdateEventStream(Func<byte[], bool> editCallback)
            {
                _manager.UpdateEventStream(MutexesKind, Name, 32, editCallback);
            }

            void UpdateEventStruct(Func<LockStructure, LockStructure?> editCallback)
            {
                UpdateEventStream(buffer =>
                {
                    int ttl = _manager.GetTickCount();

                    var inL = new LockStructure();
                    inL.Ttl = BitConverter.ToInt32(buffer, 0);
                    inL.RefCount = BitConverter.ToInt32(buffer, 4);
                    inL.LockTtl = BitConverter.ToInt32(buffer, 12);
                    var lockGuid = new byte[16]; Array.Copy(buffer, 16, lockGuid, 0, lockGuid.Length);
                    inL.LockGuid = new Guid(lockGuid);

                    if (inL.RefCount <= 0)
                    {
                        inL = new LockStructure();
                    }

                    int refreshTime = ttl - inL.Ttl;
                    if (inL.Ttl != 0)
                    {
                        if (refreshTime >= TimeoutInterval || refreshTime <= -TimeTravelInterval)
                        {
                            Debug.WriteLine(string.Format("{0} : Mutex Timed Out", Name));
                            inL = new LockStructure();
                        }
                    }

                    int lockTime = ttl - inL.LockTtl;
                    if (inL.LockGuid != Guid.Empty || inL.LockTtl != 0)
                    {
                        if (lockTime >= TimeoutInterval || lockTime < -TimeTravelInterval)
                        {
                            Debug.WriteLine(string.Format("{0} : Mutex Lock Timed Out", Name));
                            inL.LockGuid = Guid.Empty; inL.LockTtl = 0;
                        }
                    }

                    var outLmaybe = editCallback(inL);
                    if (outLmaybe == null) { return false; }
                    var outL = outLmaybe.Value;

                    outL.Ttl = ttl;
                    if (outL.LockGuid == _guid) { outL.LockTtl = ttl; }

                    Array.Copy(BitConverter.GetBytes(outL.Ttl), 0, buffer, 0, 4);
                    Array.Copy(BitConverter.GetBytes(outL.RefCount), 0, buffer, 4, 4);
                    Array.Copy(BitConverter.GetBytes(outL.LockTtl), 0, buffer, 12, 4);
                    Array.Copy(outL.LockGuid.ToByteArray(), 0, buffer, 16, 16);
                    return false; // Mutexes don't need to update a FileSystemWatcher.
                });
            }

            public override bool CreatedNew
            {
                get { return _createdNew; }
            }
        }

        Dictionary<object, Action> _jobs;
        ManualResetEvent _jobThreadReady;
        Thread _jobThread;

        public PosixEventManager()
        {
            SyncRoot = new object();
            NativeMethods = CreateNativeMethods();
        }

        internal override void Start()
        {
            _jobs = new Dictionary<object, Action>();
            _jobThreadReady = new ManualResetEvent(false);
            _jobThread = new Thread(RunJobThread) { IsBackground = true, Name = "HID System Events Job Manager" };
            _jobThread.Start();
            _jobThreadReady.WaitOne();
        }

        public override SystemEvent CreateEvent(string name)
        {
            return new PosixEvent(this, name);
        }

        public override SystemMutex CreateMutex(string name)
        {
            return new PosixMutex(this, name);
        }

        protected abstract PosixNativeMethods CreateNativeMethods();

        internal unsafe void UpdateEventStream(string kind, string name, int length, Func<byte[], bool> editCallback)
        {
            // 2017/06/27: SO DUMB. Find a better way. There has GOT to be a reliable-ish one. SO DUMB. :P
            // 2017/06/28: On the plus side, this does work reliably. So maybe it's good enough for now.
            foreach (string directory in GetEventDirectoryParts(kind))
            {
                try { Directory.CreateDirectory(directory); }
                catch { }

                try { NativeMethods.retry(() => NativeMethods.chmod(directory, 7 << 6 | 7 << 3 | 7 << 0)); }
                catch { }
            }

            string eventName = GetEventFilename(kind, name);
            using (var stream = File.Open(eventName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                int streamHandle = (int)stream.SafeFileHandle.DangerousGetHandle();
                try { NativeMethods.retry(() => NativeMethods.fchmod(streamHandle, 6 << 6 | 6 << 3 | 6)); }
                catch { }

                while (true)
                {
                    try { stream.Lock(0, 0); }
                    catch (IOException) { Thread.Sleep(50); continue; }
                    break;
                }

                var shmName = GetSHMFilename(kind, name);
                int shm;
                try { shm = NativeMethods.retry(() => NativeMethods.shm_open(shmName, NativeMethods.O_CREAT | NativeMethods.O_RDWR, 6 << 6 | 6 << 3 | 6)); } catch (DllNotFoundException) { shm = -1; }
                if (shm < 0) { throw new InvalidOperationException(string.Format("Failed to open shared memory {0}: {1}", shmName, NativeMethods.GetLastError())); }

                NativeMethods.retry(() => NativeMethods.fchmod(shm, 6 << 6 | 6 << 3 | 6));

                try
                {
                    NativeMethods.retry(() => NativeMethods.ftruncate(shm, length)); // Apparently, on shared memory, this only works the first time on MacOS. Good times.

                    IntPtr ptr = NativeMethods.mmap(IntPtr.Zero, (UIntPtr)length, NativeMethods.PROT_READ | NativeMethods.PROT_WRITE, NativeMethods.MAP_SHARED, shm, 0);
                    if (ptr == PosixNativeMethods.IntPtrNegativeOne)
                    {
                        throw new InvalidOperationException("Failed to map memory: " + NativeMethods.GetLastError().ToString());
                    }

                    try
                    {
                        byte[] buffer = new byte[length];
                        Marshal.Copy(ptr, buffer, 0, length);
                        bool update = editCallback(buffer);
                        Marshal.Copy(buffer, 0, ptr, length);

                        if (update)
                        {
                            RunNotify(stream, eventName, shmName);
                        }
                    }
                    finally
                    {
                        NativeMethods.munmap(ptr, (UIntPtr)length);
                    }
                }
                finally
                {
                    NativeMethods.retry(() => NativeMethods.close(shm));
                }
            }
        }

        protected abstract object CreateJobObject();

        protected abstract void RegisterJobObjectNotify(object jobObject, string eventName, string shmName);

        protected abstract void UnregisterJobObjectNotify(object jobObject);

        protected void RunJobObject(object jobObject)
        {
            Action job;
            if (_jobs.TryGetValue(jobObject, out job))
            {
                job();
            }
        }

        protected abstract void RunNotify(FileStream eventStream, string eventName, string shmName);

        void RegisterRefreshCallback(Action callback, out object jobObject, string eventName, string shmName)
        {
            jobObject = CreateJobObject();

            lock (SyncRoot)
            {
                _jobs.Add(jobObject, callback);
                RegisterJobObjectNotify(jobObject, eventName, shmName);
                Monitor.Pulse(SyncRoot);
            }
        }

        void UnregisterRefreshCallback(ref object jobObject)
        {
            lock (SyncRoot)
            {
                if (jobObject == null) { return; }
                UnregisterJobObjectNotify(jobObject);
                _jobs.Remove(jobObject); Monitor.Pulse(SyncRoot);
                jobObject = null;
            }
        }

        void RunJobThread()
        {
            _jobThreadReady.Set();

            lock (SyncRoot)
            {
                while (true)
                {
                    if (_jobs.Count == 0)
                    {
                        Monitor.Wait(SyncRoot);
                    }
                    else
                    {
                        Monitor.Exit(SyncRoot);
                        try { Thread.Sleep(5000); }
                        finally { Monitor.Enter(SyncRoot); }
                    }

                    foreach (var job in _jobs.Values.ToArray())
                    {
                        job();
                    }
                }
            }
        }

        static string GetEventDirectory(string kind)
        {
            return Path.Combine(Path.Combine(Path.Combine(Path.GetTempPath(), "HIDSharp"), "SystemEvents"), kind);
        }

        static string[] GetEventDirectoryParts(string kind)
        {
            return new[]
            {
                Path.Combine(Path.GetTempPath(), "HIDSharp"),
                Path.Combine(Path.Combine(Path.GetTempPath(), "HIDSharp"), "SystemEvents"),
                Path.Combine(Path.Combine(Path.Combine(Path.GetTempPath(), "HIDSharp"), "SystemEvents"), kind)
            };
        }

        static string GetEventFilename(string kind, string name)
        {
            string bs64 = Convert.ToBase64String(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(name))).Replace('+', '-').Replace('/', '_').Replace("=", "");
            string directory = GetEventDirectory(kind);
            string filename = Path.Combine(directory, bs64 + ".tmp");
            return filename;
        }

        static string GetSHMFilename(string kind, string name)
        {
            string shmName = string.Format("/HS.{0}.{1}", kind, Convert.ToBase64String(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(name))).Substring(0, 16).Replace('+', '-').Replace('/', '_'));
            return shmName;
        }

        int GetTickCount()
        {
            int t = NativeMethods.GetTickCount(); // Mono Environment.TickCount is not entirely consistent on MacOS. I think it differs between processes by their launch time.
            if (t == 0) { t = 1; }
            return t;
        }

        protected PosixNativeMethods NativeMethods
        {
            get;
            private set;
        }

        protected object SyncRoot
        {
            get;
            private set;
        }
    }
    #endregion

    #region Default Implementation
    internal class DefaultEventManager : EventManager
    {
        sealed class DefaultEvent : SystemEvent
        {
            bool _createdNew;
            EventWaitHandle _event;

            public DefaultEvent(string name)
                : base(name)
            {
                _event = new EventWaitHandle(false, EventResetMode.ManualReset, GetGlobalName(name), out _createdNew);
            }

            public override void Dispose()
            {
                try
                {
                    if (_event != null)
                    {
                        _event.Close();
                        _event = null;
                    }
                }
                catch
                {

                }
            }

            public override void Reset()
            {
                try { _event.Reset(); }
                catch { }
            }

            public override void Set()
            {
                try { _event.Set(); }
                catch { }
            }

            public override bool CreatedNew
            {
                get { return _createdNew; }
            }

            public override WaitHandle WaitHandle
            {
                get { return _event; }
            }
        }

        sealed class DefaultMutex : SystemMutex
        {
            bool _createdNew;
            Mutex _mutex;

            public DefaultMutex(string name)
                : base(name)
            {
                _mutex = new Mutex(false, GetGlobalName(name), out _createdNew);
            }

            public override void Dispose()
            {
                try
                {
                    if (_mutex != null)
                    {
                        _mutex.Close();
                        _mutex = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }

            protected override bool WaitOne(int timeout)
            {
                if (!_mutex.WaitOne(timeout)) { return false; }
                return true;
            }

            protected override void ReleaseMutex()
            {
                if (_mutex == null) { return; }
                _mutex.ReleaseMutex();
            }

            public override bool CreatedNew
            {
                get { return _createdNew; }
            }
        }

        static string GetGlobalName(string name)
        {
            if (name == null) { throw new ArgumentNullException(); }
            if (name.Length > 240) { name = "HIDSharp Global (" + Convert.ToBase64String(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(name))) + ")"; }
            return @"Global\" + name;
        }

        internal override void Start()
        {

        }

        public override SystemEvent CreateEvent(string name)
        {
            return new DefaultEvent(name);
        }

        public override SystemMutex CreateMutex(string name)
        {
            return new DefaultMutex(name);
        }
    }
    #endregion
}
