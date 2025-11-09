using BoboEngine.Shaders;
using ConsoleCommand;

namespace BoboEngine
{
    public static class SceneManager
    {
        public static Scene currentScene;
        public static void LoadScene()
        {
            UnloadScene(); // Unload Last Scene

            var shader = new Shader();

            List<GameObject> objects =
            [
                // Add Objects
                CreateObject("Test", "House", shader),
                //CreateObject("Test", "TestText", shader)
            ];

            //* Minecraft Scene
            //Shader stoneShader = new ShaderLit(Program.GetLocalTexturePath("stone"));

            objects.Add(CreateObject("Stone1", "Cube", shader, new(-1.5f, 0.8f, -2.0f), Float3.one, new(29.1f, 35.5f, -4.8f)));
            //objects.Add(CreateObject("Stone2", "Cube", shader, new(0, 0, -1)));
            //objects.Add(CreateObject("Stone3", "Cube", shader, new(1, 0, -1)));

            //Shader cobblestoneShader = new ShaderLit(Program.GetLocalTexturePath("cobblestone"));

            objects.Add(CreateObject("Cobblestone1", "Cube", shader, new(-1, 0, 0)));
            objects.Add(CreateObject("Cobblestone2", "Cube", shader));
            objects.Add(CreateObject("Cobblestone3", "Cube", shader, new(1, 0, 0)));

            //Shader dirtShader = new ShaderLit(Program.GetLocalTexturePath("dirt"));

            //objects.Add(CreateObject("Dirt1", "Cube", shader, new(-1, 0, 1)));
            //objects.Add(CreateObject("Dirt2", "Cube", shader, new(0, 0, 1)));
            //objects.Add(CreateObject("Dirt3", "Cube", shader, new(1, 0, 1)));

            /*
            Shader noobShader = new ShaderLit(Program.GetLocalTexturePath("noob"));
            GameObject noob = CreateObject("Noob", "noob", noobShader, new(0, 0.5f, 0), new(0.5f, 0.5f, 0.5f));
            noob.AddComponent<Rotater>();
            objects.Add(noob);
            */

            //*/

            /* Basic Scene
            GameObject cube = CreateObject("Cube", "Cube", new ShaderLit(Program.GetLocalTexturePath("dirt")));
            cube.AddComponent<Rotater>();
            objects.Add(cube);

            /*
            GameObject monkey = new("Monkey");
            Mesh monkeyMesh = monkey.AddComponent<Mesh>();
            monkeyMesh.LoadObjFile(Program.GetLocalModelPath("Monkey"));
            monkeyMesh.shader = new ShaderUnlit();
            monkey.transform.position = new(0, 0.233207f, 0);
            monkey.transform.rotation = new(-34.7876f, 0, 0);
            monkey.transform.scale = new(0.43f, 0.43f, 0.43f); 
            objects.Add(monkey);
            //*/

            Scene scene = new();
            currentScene = scene;

            scene.InitializeScene(objects.ToArray());

            // Create FreeCam Script
            scene.camera.gameObject.AddComponent<Move3DInput>();
        }
        public static void UnloadScene()
        {
            if (currentScene != null)
            {
                currentScene.DestroyAll();
                currentScene.DestroyBufferedObjects();
                currentScene = null;
            }
        }
        public static GameObject CreateObject(string objectName, string modelName, Shader shader, Float3 position, Float3 scale, Float3 rotation)
        {
            GameObject @object = new(objectName);
            @object.transform.position = position;
            @object.transform.scale = scale;
            @object.transform.rotation = rotation;

            Mesh mesh = @object.AddComponent<Mesh>();
            mesh.shader = shader;

            if (!mesh.LoadObjFile(Program.GetLocalModelPath(modelName))) return null; // If model fails to load

            return @object;
        }
        
        public static GameObject CreateObject(string objectName, string modelName, Shader shader) => CreateObject(objectName, modelName, shader, Float3.zero, Float3.one, Float3.zero);
        public static GameObject CreateObject(string objectName, string modelName, Shader shader, Float3 position) => CreateObject(objectName, modelName, shader, position, Float3.one, Float3.zero);
        public static GameObject CreateObject(string objectName, string modelName, Shader shader, Float3 position, Float3 scale) => CreateObject(objectName, modelName, shader, position, scale, Float3.zero);
        public static GameObject CreateObject(string objectName, string modelName, string textureName, Float3 position, Float3 scale, Float3 rotation)
        {
            Shader shader;

            if (!string.IsNullOrEmpty(textureName))
                throw new NotImplementedException("No textures yet!");
            else
                shader = new Shader();

            return CreateObject(objectName, modelName, shader, position, scale, rotation);
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
            currentScene.AddObject(CreateObject(modelName, modelName, textureName, new(x, y, z), Float3.one, Float3.zero));
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
