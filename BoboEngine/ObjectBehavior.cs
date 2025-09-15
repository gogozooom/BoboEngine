namespace BoboEngine
{
    public class ObjectBehavior
    {
        public GameObject gameObject;
        public Transform transform => gameObject.transform;

        /// <summary>
        /// Runs on scene initalization
        /// </summary>
        public virtual void Start()
        {
            Console.WriteLine("[ObjectBehavior Start!]");
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
        public void Destroy()
        {
            gameObject.RemoveComponent(this);
        }
    }
}
