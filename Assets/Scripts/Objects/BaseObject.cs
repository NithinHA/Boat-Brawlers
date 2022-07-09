using UnityEngine;

public class BaseObject : MonoBehaviour
{
    public float Weight = 1f;

    protected virtual void Start()
    {
        RaftController.Instance.OnObjectCreated?.Invoke(this);
    }

    protected virtual void OnDestroy()
    {
        if(RaftController.Instance != null)
            RaftController.Instance.OnObjectDestroyed?.Invoke(this);
    }
}
