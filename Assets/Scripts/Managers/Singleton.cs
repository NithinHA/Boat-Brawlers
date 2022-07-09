using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    
    public bool PersistentOnSceneChange = false;
	
    protected Singleton() { }

	public static T Instance
	{
		get
		{
			if (_instance == null)
				_instance = FindObjectOfType<T>();

			return _instance;
		}
	}

	protected virtual void Awake()
	{
		if (PersistentOnSceneChange)
			DontDestroyOnLoad(gameObject);
	}

	protected virtual void Start()
	{
		T[] instance = FindObjectsOfType<T>();
		if (instance.Length > 1)
		{
			Debug.Log(gameObject.name + " has been destroyed because another object already has the same component.");
			Destroy(gameObject);
		}
	}

	protected virtual void OnDestroy()
	{
		if (this == _instance)
			_instance = null;
	}
}