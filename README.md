# BoboEngine

A simple, *unfinished*, object oriented game engine built with C# using OpenGL.

# SETUP

Download your prefered version of BoboEngine in the [releases tab](https://github.com/gogozooom/BoboEngine/releases).

Create a new C# Console app in a separate folder with ".NET 9.0" as the framework and keep everything else default.

Right click "Dependencies" in the Solution Explorer and select "Add Project Reference", click "Browse", then navigate to the "BoboEngine.dll" you downloaded or built from source, and finally click "OK".

Right click "Dependencies" once more, then "Manage NuGet Packages". Search for "StbImageSharp" and install the latest version.

Now you can start the engine and see a few meshes with this Program.cs file:

    using BoboEngine;
    using BoboEngine.UI;
    
    internal class Program
    {
    
        static void Main(string[] args)
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            Engine.Start();
        }
        
        public static void OnSceneLoad()
        {
            // Define a texture and material
            var NullTexture = new Texture2D(Engine.GetLocalTexturePath("NULL"), "NULL", TextureSampleType.Nearest);
            var mat = new Material("default", NullTexture);
    
            // Use the automatic CreateObjectModel method to automatically load default model objects
            var plane = SceneManager.CreateObjectModel("Plane", "Plane", mat);
            var cube = SceneManager.CreateObjectModel("Cube", "Cube", mat);
    
            // Add a default test Rotater component
            cube.AddComponent<Rotater>();
    
            // Manually create a new GameObject
            var uiTest = new GameObject("UI");
            // Add a UIRenderer componenet
            var uiRender = uiTest.AddComponent<UIRenderer>();
    
            // Assign a new material that will be gaurenteed to render over everythine else in the scene
            uiRender.material = new Material("defaultUI", NullTexture, true, false, BlendMode.Normal, renderOrder: 1000);
            // Anchor sprite to take up a small portion of the bottom left
            uiRender.uiTransform.anchor = new UVRect(0.1f, 0.1f, 0.2f, 0.2f); 
            // Add an extra 16 px of scale offset
            uiRender.uiTransform.scale = new Float2(16, 16);  
    
            // Give the sky a nice dark blue color
            SceneManager.currentScene.skybox.skyColor = new Float3(0.1f, 0.2f, 0.3f);
        }
    }

Running this for the first time will give an error however, and that is because we are missing the "Bin-Dep" files from the cloned project.

Copy all contests of the "Bin-Dep" folder, and paste them in the same directory as your newly built .exe file.

Running your project one last time should now show all the test objects we created!

# BUILD FROM SOURCE

Clone this repo and opening the project in Visual Studio, opening the .sln file, then right click the "BoboEngine" root in solution explorer and select "Build".
