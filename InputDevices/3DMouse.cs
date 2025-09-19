using BoboEngine.GMath;
using ConsoleCommand;
using HidSharp;
using HidSharp.Reports;

namespace InputDevices
{
    // Possible TODO: Extends off of InputDevice base class
    public static class _3DMouse
    {
        public static _3DInput input { get; private set; } = new();

        const int ID_VENDOR = 0x256F;
        const int ID_PRODUCT = 0xC635;

        static HidDevice inputDevice;
        static HidStream inputStream;

        [Command("InitMouse", "Initalizes 3DMouse Device!")]
        public static bool Initalize()
        {
            var list = DeviceList.Local;

            string targetID = GetFormattedID();

            foreach (var device in list.GetHidDevices())
            {
                //Program.Log($" * [{device.GetFriendlyName()}] Properties:");

                string[] deviceInfo = device.DevicePath.Split("#");

                if (deviceInfo.Length < 2) continue; 

                string deviceID = deviceInfo[1];

                if (deviceID.Equals(targetID, StringComparison.OrdinalIgnoreCase))
                {
                    inputDevice = device;
                    break;
                }
            }

            if (inputDevice == null)
            {
                Program.LogError($"Could not find targetID = '{targetID}'");
                return false;
            }

            bool success = inputDevice.TryOpen(out inputStream);

            if (!success)
            {
                Program.LogError($"Could not open inputStream!");
                return false;
            }

            return true;
        }

        static Thread readInputLoop;

        [Command("GetMouse", "Prints the current input")]
        public static void GetInput()
        {
            Program.LogMessage(input);
        }
        [Command("ReadMouse", "Reads mouse input")]
        public static void StartReadingInput()
        {
            if (readInputLoop != null)
            {
                if (readInputLoop.IsAlive)
                {
                    Program.LogWarning("Already Reading Input!");
                    return;
                }
            }

            readInputLoop = new Thread(ReadInputLoop);

            readInputLoop.Start();
        }
        static void ReadInputLoop()
        {
            if (inputStream == null)
            {
                if (!Initalize())
                {
                    return;
                }
            }

            var reportDescriptor = inputDevice.GetReportDescriptor();

            var inputReportBuffer = new byte[inputDevice.GetMaxInputReportLength()];
            var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
            var inputParser = reportDescriptor.DeviceItems[0].CreateDeviceItemInputParser();

            inputReceiver.Start(inputStream);

            int startTime = Environment.TickCount;
            bool posInput = true;
            while (true)
            {
                if (inputReceiver.WaitHandle.WaitOne(1000))
                {
                    if (!inputReceiver.IsRunning) // Disconnected?
                    {
                        Program.LogError("DEVICE DISCONNECTED WHILE READING INPUT!");

                        inputDevice = null;
                        inputStream = null;

                        break;
                    } 

                    Report report;
                    while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                    {
                        // Parse the report if possible.
                        // This will return false if (for example) the report applies to a different DeviceItem.


                        if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                        {
                            string totalMessage = "Data:\n";

                            List<DataValue> data = new();

                            for (int i = 0; i < inputParser.ValueCount; i++)
                            {
                                var value = inputParser.GetValue(i);

                                totalMessage += $"  [{i}] '{value.DataItem.ElementBits}' '{value.GetLogicalValue()}'\n";

                                data.Add(value);
                            }

                            totalMessage += $"\nEnd; PosInput = '{posInput}'";

                            //Program.LogMessage(input);

                            if (data[0].DataItem.ExpectedUsageType == ExpectedUsageType.PushButton)
                            {
                                input.SetButtonInput(data[0].GetLogicalValue() == 1, data[1].GetLogicalValue() == 1);
                            }
                            else
                            {
                                if (posInput) // Hacky solution TODO: (Has problem of X inputs randomly getting swapped, better distinction solution to be found)
                                {
                                    Float3 pos = new(ConvertInput(data[0].GetLogicalValue()), ConvertInput(data[4].GetLogicalValue()), ConvertInput(data[3].GetLogicalValue())); // [6,7] Are still readable?

                                    input.SetPositionInput(pos);
                                    posInput = false;
                                }
                                else
                                {
                                    Float3 rot = new(ConvertInput(data[0].GetLogicalValue()), ConvertInput(data[7].GetLogicalValue()), ConvertInput(data[6].GetLogicalValue())); // But this makes more sense

                                    input.SetRotationInput(rot);
                                    posInput = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        #region Background
        public static string ToHex(this int value)
        {
            return String.Format("{0:X}", value).ToLower();
        }
        static float ConvertInput(int input)
        {
            float result = 0;

            if (input <= 350) // Negative
            {
                result = -input/350f;
            }
            else // Positive
            {
                result = (65535 - input)/349f;
            }

            return result;
        }
        static string GetFormattedID()
        {
            return $"vid_{ID_VENDOR.ToHex()}&pid_{ID_PRODUCT.ToHex()}";
        }
        #endregion
    }
    public class _3DInput
    {
        public Float3 position { get; private set; }
        public Float3 rotation { get; private set; }
        public bool leftPressed { get; private set; }
        public bool rightPressed { get; private set; }
        public Action<bool> onLeftInput;
        public Action<bool> onRightInput;

        public _3DInput()
        {
            position = Float3.zero;
            rotation = Float3.zero;
            leftPressed = false;
            rightPressed = false;
        }
        public _3DInput(Float3 position, Float3 rotation, bool leftPressed, bool rightPressed)
        {
            this.position = position;
            this.rotation = rotation;
            this.leftPressed = leftPressed;
            this.rightPressed = rightPressed;
        }

        public void SetPositionInput(Float3 position)
        {
            this.position = position;
        }
        public void SetRotationInput(Float3 rotation)
        {
            this.rotation = rotation;
        }
        public void SetButtonInput(bool leftPressed, bool rightPressed)
        {
            if (!this.leftPressed && leftPressed || this.leftPressed && !leftPressed)
            {
                onLeftInput?.Invoke(leftPressed);
            }
            if (!this.rightPressed && rightPressed || this.rightPressed && !rightPressed)
            {
                onRightInput?.Invoke(rightPressed);
            }


            this.leftPressed = leftPressed;
            this.rightPressed = rightPressed;
        }

        public override string ToString()
        {
            return $"pos: [{position}] rot: [{rotation}] buttons: [{leftPressed},{rightPressed}]";
        }
    }
}