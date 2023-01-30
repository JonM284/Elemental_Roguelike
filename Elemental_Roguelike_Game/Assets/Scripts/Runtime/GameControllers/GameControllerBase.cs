using UnityEngine;

namespace Runtime.GameControllers
{
    public class GameControllerBase : MonoBehaviour , IController
    {
        public bool is_Initialized { get; private set; }
        
        public virtual void Initialize()
        {
            is_Initialized = true;
            Debug.Log($"{this.name} is initialized");
        }

        public virtual void Cleanup()
        {
            is_Initialized = false;
        }
    }
}