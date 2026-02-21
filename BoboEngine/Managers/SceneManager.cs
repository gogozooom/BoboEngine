using ConsoleCommand;

namespace BoboEngine;
public static class SceneManager
{
    public static Scene currentScene;
    public static Action sceneLoaded;
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

        scene.camera.transform.SetRotation(new Float3(0, 0, 0));
        scene.camera.transform.position = new Float3(0, 1.1f, -5.5f);

        sceneLoaded?.Invoke();

        /* Block Look Direction Testing
        var obj = new GameObject("Debug");
        obj.transform.scale = -Float3.one;
        obj.transform.position = new(-0.5f, 1.5f, 0.5f);

        var mesh = obj.AddComponent<Mesh>();

        mesh.LoadObjFile(Program.GetLocalModelPath("Cube"));
        mesh.shader = new Shader(new Texture(Program.GetLocalTexturePath("NULL")));
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
    
    private static Material GetLocalMaterialWithTexture(string textureName)
    {
        Material material;

        if (!string.IsNullOrEmpty(textureName))
        {
            var texture = new Texture2D(Engine.GetLocalTexturePath(textureName), textureName);

            material = new Material(texture: texture);
        }
        else
            material = new Material();

        return material;
    }
    private static Mesh GetLocalModelWithName(string modelName)
    {
        Mesh mesh = new Mesh();
        if (!mesh.LoadObjFile(Engine.GetLocalModelPath(modelName))) // If model fails to load
        {
            return null;
        }

        return mesh;
    }
    public static GameObject CreateObjectModel(string objectName, Mesh mesh, Material material)
    {
        GameObject @object = new(objectName);

        MeshFilter meshFilter = @object.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = @object.AddComponent<MeshRenderer>();
        meshRenderer.material = material;

        return @object;
    }
    public static GameObject CreateObjectModel(string objectName, Mesh mesh, string textureName)
    {
        var material = GetLocalMaterialWithTexture(textureName);

        return CreateObjectModel(objectName, mesh, material);
    }
    public static GameObject CreateObjectModel(string objectName, string modelName, Material material)
    {
        var mesh = GetLocalModelWithName(modelName);

        if (!mesh) return null;

        return CreateObjectModel(objectName, mesh, material);
    }

    public static GameObject CreateObjectModel(string objectName, string modelName, string textureName)
    {
        var material = GetLocalMaterialWithTexture(textureName);

        return CreateObjectModel(objectName, modelName, material);
    }

    #region Commands
    [Command("RemoveObject", "['name'] Deletes the specified object")]
    public static void RemoveObject(string modelName)
    {
        GameObject obj = currentScene.Find(modelName);

        if (obj == null)
        {
            Engine.LogError($"Could not find '{modelName}'! Try using one of the following:");

            foreach (var _obj in currentScene.objects)
            {
                Engine.LogMessage($" - '{_obj.name}'");
            }

            return;
        }

        obj.Destroy();
    }
    [Command("SpawnMesh", "['name', 'meshName', 'textureName', x, y, z] Creates a new mesh object at the specified position")]
    public static void SpawnObject(string objectName, string meshName, string textureName, float x, float y, float z)
    {
        var obj = CreateObjectModel(objectName, meshName, textureName);
        if(obj) obj.transform.position = new(x, y, z);
    }

    [Command("RotateObject", "['name', x, y, z] Sets the selected model to the specified rotation")]
    public static void SetObjectRotation(string name, float x, float y, float z)
    {
        GameObject obj = currentScene.Find(name);

        if (obj == null)
        {
            Engine.LogError($"Could not find '{name}'! Try using one of the following:");

            foreach (var _obj in currentScene.objects)
            {
                Engine.LogMessage($" - '{_obj.name}'");
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
            Engine.LogError($"Could not find '{name}'! Try using one of the following:");

            foreach (var _obj in currentScene.objects)
            {
                Engine.LogMessage($" - '{_obj.name}'");
            }

            return;
        }

        obj.transform.position = new(x, y, z);
    }
    [Command("ScaleObject", "['name', x, y, z] Sets the selected model to the specified scale")]
    public static void SetObjectScale(string name, float x, float y, float z)
    {
        GameObject obj = currentScene.Find(name);

        if (obj == null)
        {
            Engine.LogError($"Could not find '{name}'! Try using one of the following:");

            foreach (var _obj in currentScene.objects)
            {
                Engine.LogMessage($" - '{_obj.name}'");
            }

            return;
        }

        obj.transform.scale = new(x, y, z);
    }
    #endregion
}