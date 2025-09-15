
using HidSharp.Reports;

namespace BoboEngine
{
    public class Scene
    {
        public List<GameObject> objects = new();
        public Camera camera;

        public Scene(GameObject[] objects)
        {
            foreach (var obj in objects)
            {
                AddObject(obj);
            }

            GameObject cameraObject = new("Camera");
            camera = cameraObject.AddComponent<Camera>();

            AddObject(cameraObject);
            // TODO: make scene be able to load and save from file format!
        }
        /// <summary>
        /// Updates all objects in the scene
        /// </summary>
        public void Update()
        {
            foreach (var item in objects)
            {
                UpdateObject(item);
            }
        }
        public GameObject Find(string name)
        {
            foreach (var obj in objects)
            {
                if (obj.name == name)
                {
                    return obj;
                }
            }
            return null;
        }
        /// <summary>
        /// Destroys all objects in scene
        /// </summary>
        public void Destroy()
        {
            foreach (var item in objects)
            {
                item.Destroy();
            }
        }
        /// <summary>
        /// Adds an object to the scene
        /// </summary>
        /// <param name="_object">The object to instantiate</param>
        public void AddObject(GameObject _object)
        {
            objects.Add(_object);

            _object.connectedScene = this;

            StartObject(_object);
        }
        public void RemoveObject(GameObject _object)
        {
            objects.Remove(_object);

            _object.OnDestroy();
        }
        void StartObject(GameObject gObject)
        {
            foreach (var component in gObject.components)
            {
                component.Start();
            }
        }
        void UpdateObject(GameObject gObject)
        {
            foreach (var component in gObject.components)
            {
                component.Update();
            }
        }
    }
}
