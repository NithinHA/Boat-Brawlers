using UnityEngine;

namespace BaseObjects
{
    public class BaseObject : MonoBehaviour
    {
        public float Weight = 1f;

        protected virtual void Start()
        {
            if (RaftController.Instance != null)
                RaftController.Instance.OnObjectCreated?.Invoke(this);

            if (RaftController_Custom.Instance != null)
                RaftController_Custom.Instance.OnObjectCreated?.Invoke(this);
        }

        protected virtual void OnDestroy()
        {
            if (RaftController.Instance != null)
                RaftController.Instance.OnObjectDestroyed?.Invoke(this);
            
            if (RaftController_Custom.Instance != null)
                RaftController_Custom.Instance.OnObjectDestroyed?.Invoke(this);
        }
    }
}
