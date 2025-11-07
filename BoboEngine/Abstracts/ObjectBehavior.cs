namespace BoboEngine
{
    public abstract class ObjectBehavior
    {
        public GameObject gameObject;
        public Transform transform => gameObject.transform;

        /// <summary>
        /// Runs on scene initalization
        /// </summary>
        public virtual void Start()
        {
            Program.Log($"'{gameObject}' Start");
        }
        /// <summary>
        /// Runs every frame
        /// </summary>
        public virtual void Update()
        {

        }

        /// <summary>
        /// Runs right after getting destroyed
        /// </summary>
        public virtual void OnDestroy()
        {

        }
        /// <summary>
        /// Destroys the component
        /// </summary>
        public void DestroyImmediate()
        {
            gameObject.RemoveComponent(this);
        }

        /// <summary>
        /// parents to be implemented
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public GameObject Find(string name)
        {
            throw new NotImplementedException("parents to be implemented");
        }

        public static implicit operator bool (ObjectBehavior o) => o != null;
    }
}
