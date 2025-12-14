namespace BoboEngine
{
    public class Scene
    {
        public Camera camera;
        public List<GameObject> objects = new();
        public Action<GameObject> ObjectAdded;
        public Action<GameObject> DestroyingObject;

        private List<GameObject> _objectsBufferedForRemoval = new();

        public void InitializeScene(GameObject[] objects)
        {
            GameObject cameraObject = new("Camera");
            camera = cameraObject.AddComponent<Camera>();

            foreach (var obj in objects)
            {
                AddObject(obj);
            }

            WindowManager.update += Update;
            WindowManager.afterUpdate += LateUpdate;
        }
        public GameObject Find(string name)
        {
            foreach (var obj in objects.ToArray())
            {
                if (obj.name == name)
                {
                    return obj;
                }
            }
            return null;
        }
        /// <summary>
        /// Destroys all objects in scene next frame
        /// </summary>
        public void DestroyAll()
        {
            foreach (var item in objects)
            {
                DestroyObject(item);
            }
        }
        /// <summary>
        /// Destroys all buffered objects for removal
        /// </summary>
        public void DestroyObject(GameObject gameObject)
        {
            if(!_objectsBufferedForRemoval.Contains(gameObject))
                _objectsBufferedForRemoval.Add(gameObject);
        }
        public void DestroyBufferedObjects()
        {
            foreach (var obj in _objectsBufferedForRemoval)
            {
                obj.OnDestroy();
            }

            _objectsBufferedForRemoval.Clear();
        }
        /// <summary>
        /// Adds an object to the scene
        /// </summary>
        /// <param name="_object">The object to instantiate</param>
        public void AddObject(GameObject _object)
        {
            if (_object == null) return;
            if (objects.Contains(_object))
            {
                Program.LogWarning($"Already have '{_object}' !");
                return;
            }


            objects.Add(_object);

            _object.connectedScene = this;
                
            ObjectAdded?.Invoke(_object);
        }
        public void RemoveObject(GameObject _object)
        {
            DestroyingObject?.Invoke(_object);
            objects.Remove(_object);

            _object.OnDestroy();
        }

        /// <summary>
        /// Updates all objects in the scene
        /// </summary>
        private void Update()
        {
            foreach (var item in objects.ToArray())
            {
                UpdateObject(item);
            }
        }

        /// <summary>
        /// Late updates all objects in the scene
        /// </summary>
        private void LateUpdate()
        {
            DestroyBufferedObjects();

            foreach (var item in objects.ToArray())
            {
                LateUpdateObject(item);
            }
        }

        // TODO: Will be used when separated with an AWAKE method
        // Awake method will be run as soon as a compoment is added,
        // Start is run after all objects in the scene are finnished loading
        private void StartObject(GameObject gObject)
        {
            foreach (var component in gObject.components)
            {
                component.Start();
            }
        }
        private void UpdateObject(GameObject gObject)
        {
            foreach (var component in gObject.components)
            {
                component.Update();
            }
        }
        private void LateUpdateObject(GameObject gObject)
        {
            foreach (var component in gObject.components)
            {
                component.LateUpdate();
            }
        }
    }
}
