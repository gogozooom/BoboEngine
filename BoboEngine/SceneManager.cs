using BoboEngine.Shaders;
using ConsoleCommand;

namespace BoboEngine;
public static class SceneManager
{
    public static Scene currentScene;
    public static void LoadScene()
    {
        UnloadScene(); // Unload Last Scene


        List<GameObject> objects =
        [
            // Add Objects
            //CreateObject("Test", "House", shader),
            //CreateObject("Test", "TestText", shader),
        ];

        Scene scene = new();
        currentScene = scene;

        scene.InitializeScene(objects.ToArray());

        // Create FreeCam Script
        scene.camera.gameObject.AddComponent<Move3DInput>();
        scene.camera.transform.SetRotation(new Float3(0, 0, 0));
        scene.camera.transform.position = new Float3(0, 1.1f, -5.5f);
        
        /* Block Look Direction Testing
        var obj = new GameObject("Debug");
        obj.transform.scale = -Float3.one;
        obj.transform.position = new(-0.5f, 1.5f, 0.5f);

        var mesh = obj.AddComponent<Mesh>();

        mesh.LoadObjFile(Program.GetLocalModelPath("Cube"));
        mesh.shader = new Shader(new Texture(Program.GetLocalTexturePath("NULL")));
        */

        /* Texture Array Testing
        // Test 1
        var test1 = new GameObject("TextureArrayTest1");
        test1.transform.position = new(-2, 3, 0);

        var test1Mesh = test1.AddComponent<Mesh>();
        test1Mesh.LoadObjFile(Program.GetLocalModelPath("Cube"));

        var image = new Float3[,]
        {
            { new(1, 0, 0), new(0, 0, 0), new(0, 0, 0), new(0.25f, 0.25f, 0.25f) },
            { new(0, 0, 0), new(0, 1, 0), new(0, 0, 0), new(0.5f, 0.5f, 0.5f) },
            { new(0, 0, 0), new(0, 0, 0), new(0, 0, 1), new(0.75f, 0.75f, 0.75f) },
            { new(0.25f, 0.25f, 0.25f), new(0.5f, 0.5f, 0.5f), new(0.75f, 0.75f, 0.75f), new(1, 1, 1) }
        };

        var texture = new Texture(image, "test1");

        var shader = new Shader(texture);

        test1Mesh.shader = shader;


        // Test 2
        var test2 = new GameObject("TextureArrayTest2");
        test2.transform.position = new(2, 3, 0);

        var test2Mesh = test2.AddComponent<Mesh>();
        test2Mesh.LoadObjFile(Program.GetLocalModelPath("Cube"), 1);

        var image2 = new Float3[,]
{
            { new(1, 0, 1), new(0, 0, 1), new(0, 0, 1), new(0.25f, 0.25f, 0.25f) },
            { new(0, 0, 1), new(0, 1, 1), new(0, 0, 1), new(0.5f, 0.5f, 0.5f) },
            { new(0, 0, 1), new(0, 0, 1), new(0, 0, 1), new(0.75f, 0.75f, 0.75f) },
            { new(0.25f, 0.25f, 0.25f), new(0.5f, 0.5f, 0.5f), new(0.75f, 0.75f, 0.75f), new(1, 1, 1) }
        };

        var texture2 = new Texture([image, image2], "test2");

        var shader2 = new Shader(texture2);

        test2Mesh.shader = shader2;
        */
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
    public static GameObject CreateObjectModel(string objectName, string modelName, Shader shader, Float3 position, Float3 scale, Float3 rotation)
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
        
    public static GameObject CreateObjectModel(string objectName, string modelName, Shader shader) => CreateObjectModel(objectName, modelName, shader, Float3.zero, Float3.one, Float3.zero);
    public static GameObject CreateObjectModel(string objectName, string modelName, Shader shader, Float3 position) => CreateObjectModel(objectName, modelName, shader, position, Float3.one, Float3.zero);
    public static GameObject CreateObjectModel(string objectName, string modelName, Shader shader, Float3 position, Float3 scale) => CreateObjectModel(objectName, modelName, shader, position, scale, Float3.zero);
    public static GameObject CreateObjectModel(string objectName, string modelName, string textureName, Float3 position, Float3 scale, Float3 rotation)
    {
        Shader shader;

        if (!string.IsNullOrEmpty(textureName))
        {
            var texture = new Texture(Program.GetLocalTexturePath(textureName));

            shader = new Shader(texture);
        }
        else
            shader = new Shader();

        return CreateObjectModel(objectName, modelName, shader, position, scale, rotation);
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

        obj.OnDestroy();
    }
    [Command("SpawnMesh", "['name', 'textureName', x, y, z] Creates a new mesh object at the specified position")]
    public static void SpawnObject(string modelName, string textureName, float x, float y, float z)
    {
        currentScene.AddObject(CreateObjectModel(modelName, modelName, textureName, new(x, y, z), Float3.one, Float3.zero));
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