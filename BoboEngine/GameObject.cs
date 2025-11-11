namespace BoboEngine;
public class GameObject
{
    public string name;
    public Transform transform = new();
    public Scene connectedScene;

    public List<ObjectBehavior> components = new();

    public GameObject(string name = "GameObject")
    {
        this.name = name;
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
        foreach (var com in components)
        {
            if (com.GetType() == typeof(T))
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
}