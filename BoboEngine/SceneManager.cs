using BoboEngine.Shaders;
using ConsoleCommand;

namespace BoboEngine
{
    public static class SceneManager
    {
        public static Scene currentScene;
        public static void LoadScene()
        {
            if (currentScene != null)
            {
                currentScene.Destroy();
                currentScene = null;
            }

            List<GameObject> objects = new();

            // Add Objects

            //* Minecraft Scene
            objects.Add(CreateObject("Stone1", "Cube", "stone", new(-1, 0, -1)));
            objects.Add(CreateObject("Stone2", "Cube", "stone", new(0, 0, -1)));
            objects.Add(CreateObject("Stone3", "Cube", "stone", new(1, 0, -1)));

            objects.Add(CreateObject("Cobblestone1", "Cube", "cobblestone", new(-1, 0, 0)));
            objects.Add(CreateObject("Cobblestone2", "Cube", "cobblestone", new(0, 0, 0)));
            objects.Add(CreateObject("Cobblestone3", "Cube", "cobblestone", new(1, 0, 0)));

            objects.Add(CreateObject("Dirt1", "Cube", "dirt", new(-1, 0, 1)));
            objects.Add(CreateObject("Dirt2", "Cube", "dirt", new(0, 0, 1)));
            objects.Add(CreateObject("Dirt3", "Cube", "dirt", new(1, 0, 1)));
            //*/

            /* Basic Scene
            GameObject plane = new("Plane");
            Mesh planeMesh = plane.AddComponent<Mesh>();
            planeMesh.LoadObjFile(Program.GetLocalModelPath("Plane"));
            planeMesh.shader = new ShaderUnlit();
            objects.Add(plane);

            GameObject monkey = new("Monkey");
            Mesh monkeyMesh = monkey.AddComponent<Mesh>();
            monkeyMesh.LoadObjFile(Program.GetLocalModelPath("Monkey"));
            monkeyMesh.shader = new ShaderUnlit();
            monkey.transform.position = new(0, 0.233207f, 0);
            monkey.transform.rotation = new(-34.7876f, 0, 0);
            monkey.transform.scale = new(0.43f, 0.43f, 0.43f); 
            objects.Add(monkey);
            //*/

            Scene scene = new(objects.ToArray());

            // Create FreeCam Script
            scene.camera.gameObject.AddComponent<FreeCam>();
            scene.camera.transform.position = new(0, 1, 2);
            scene.camera.transform.rotation = new(20, 180, 0);

            currentScene = scene;
        }

        public static GameObject CreateObject(string objectName, string modelName, string textureName = "", Float3 position = new(), Float3 rotation = new())
        {
            GameObject @object = new(objectName);
            @object.transform.position = position;
            @object.transform.rotation = rotation;

            Mesh mesh = @object.AddComponent<Mesh>();

            if (!string.IsNullOrEmpty(textureName))
                mesh.shader = new ShaderUnlit(Program.GetLocalTexturePath(textureName));
            else
                mesh.shader = new ShaderUnlit();

            if (!mesh.LoadObjFile(Program.GetLocalModelPath(modelName))) return null; // If model fails to load

            return @object;
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
        [Command("SpawnMesh", "['name', 'textureName', x, y, z] Creates a new mesh object at the specified position")]
        public static void SpawnObject(string modelName, string textureName, float x, float y, float z)
        {
            currentScene.AddObject(CreateObject(modelName, modelName, textureName, new(x, y, z)));
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
