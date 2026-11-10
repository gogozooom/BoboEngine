namespace BoboEngine;
public abstract class ObjectBehavior
{
    public GameObject gameObject;
    public Transform transform => gameObject.transform;

    /// <summary>
    /// Runs on scene initalization
    /// </summary>
    public virtual void Start(){}

    /// <summary>
    /// Runs every frame
    /// </summary>
    public virtual void Update(){}

    /// <summary>
    /// Runs after the frame has rendered
    /// </summary>
    public virtual void LateUpdate(){}

    /// <summary>
    /// Runs right after getting destroyed
    /// </summary>
    public virtual void OnDestroy(){}

    public static implicit operator bool (ObjectBehavior o) => o != null;
}