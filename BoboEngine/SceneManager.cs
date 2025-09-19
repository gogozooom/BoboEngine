using ConsoleCommand;

namespace BoboEngine
{
    public static class SceneManager
    {
        public static Scene currentScene;
        public static void LoadScene() // TMP
        {
            if (currentScene != null)
            {
                currentScene.Destroy();
                currentScene = null;
            }

            GameObject plane = new("Plane");
            Mesh planeMesh = plane.AddComponent<Mesh>();
            planeMesh.LoadObjFile(Mesh.GetLocalModelPath("Plane"));

            GameObject monkey = new("Monkey");
            Mesh monkeyMesh = monkey.AddComponent<Mesh>();
            monkeyMesh.LoadObjFile(Mesh.GetLocalModelPath("Monkey"));
            monkey.transform.position = new(0, 0.233207f, 0);
            monkey.transform.rotation = new(-34.7876f, 0, 0);
            monkey.transform.scale = new(0.43f, 0.43f, 0.43f);

            Scene scene = new([plane, monkey]);

            scene.camera.gameObject.AddComponent<FreeCam>();
            scene.camera.transform.position = new(0, 1, 2);
            scene.camera.transform.rotation = new(20, 180, 0);

            currentScene = scene;
        }

        [Command("RemoveObject", "['name'] Deletes the specified object")]
        public static void RemoveObject(string modelName)
        {
            GameObject obj = currentScene.Find(modelName);

            if (obj == null)
            {
                Program.LogError($"Could not find '{modelName}'! Try using one of the following:");

                foreach (var _obj in currentScene.objects)
                {
                    Program.LogMessage($" - '{_obj.name}'");
                }

                return;
            }

            obj.Destroy();
        }
        [Command("SpawnMesh", "['name', x, y, z] Creates a new mesh object at the specified position")]
        public static void SpawnObject(string modelName, float x, float y, float z)
        {
            GameObject model = new(modelName);
            model.transform.position = new(x, y, z);

            Mesh mesh = model.AddComponent<Mesh>();
            if (!mesh.LoadObjFile(Mesh.GetLocalModelPath(modelName))) return; // If model fails to load

            currentScene.AddObject(model);
        }

        [Command("RotateObject", "['name', x, y, z] Sets the selected model to the specified rotation")]
        public static void SetObjectRotation(string name, float x, float y, float z)
        {
            GameObject obj = currentScene.Find(name);

            if (obj == null)
            {
                Program.LogError($"Could not find '{name}'! Try using one of the following:");

                foreach (var _obj in currentScene.objects)
                {
                    Program.LogMessage($" - '{_obj.name}'");
                }

                return;
            }

            obj.transform.rotation = new(x, y, z);
        }
        [Command("MoveObject", "['name', x, y, z] Sets the selected model to the specified position")]
        public static void SetObjectPosition(string name, float x, float y, float z)
        {
            GameObject obj = currentScene.Find(name);

            if (obj == null)
            {
                Program.LogError($"Could not find '{name}'! Try using one of the following:");

                foreach (var _obj in currentScene.objects)
                {
                    Program.LogMessage($" - '{_obj.name}'");
                }

                return;
            }

            obj.transform.position = new(x,y,z);
        }
        [Command("ScaleObject", "['name', x, y, z] Sets the selected model to the specified scale")]
        public static void SetObjectScale(string name, float x, float y, float z)
        {
            GameObject obj = currentScene.Find(name);

            if (obj == null)
            {
                Program.LogError($"Could not find '{name}'! Try using one of the following:");

                foreach (var _obj in currentScene.objects)
                {
                    Program.LogMessage($" - '{_obj.name}'");
                }

                return;
            }

            obj.transform.scale = new(x, y, z);
        }
    }
}
