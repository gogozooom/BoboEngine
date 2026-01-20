namespace BoboEngine;
public class GameObject
{
    public bool enabled = true;
    public string name;
    public Transform transform;
    public Scene connectedScene;

    public List<ObjectBehavior> components = new();

    public GameObject(string name = "GameObject")
    {
        if(SceneManager.currentScene != null)
        {
            SceneManager.currentScene.AddObject(this);
        }
        else
        {
            Engine.LogWarning($"Adding Object '{name}'!");
        }

        this.name = name;
        enabled = true;

        transform = new();
        components.Add(transform);
        transform.gameObject = this;
    }

    public T AddComponent<T>() where T : ObjectBehavior, new()
    {
        T t = new T
        {
            gameObject = this
        };

        components.Add(t);
        t.Start();

        return t;
    }
    public T GetComponent<T>() where T : ObjectBehavior
    {
        foreach (var com in components.ToArray())
        {
            if (com.GetType() == typeof(T) || com.GetType().IsSubclassOf(typeof(T)))
            {
                return (T)com;
            }
        }

        return null;
    }
    public void RemoveComponent(ObjectBehavior component)
    {
        components.Remove(component);

        component.OnDestroy();
    }

    public static GameObject Find(string name)
    {
        return SceneManager.currentScene?.Find(name);
    }
    public void Destroy()
    {
        connectedScene.DestroyObject(this);
    }
    /// <summary>
    /// Runs right before getting destroyed
    /// </summary>
    public void OnDestroy()
    {
        foreach (ObjectBehavior component in components.ToArray())
        {
            RemoveComponent(component);
        }
    }

    public override string ToString()
    {
        return $"GameObject '{name}'";
    }

    public static implicit operator bool (GameObject o) => o != null;
}