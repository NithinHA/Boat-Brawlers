using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : Singleton<MainMenuController>
{
    [SerializeField] private RaftObjectMap[] m_RaftsInScene;

    private readonly Dictionary<RaftType, GameObject> _raftMap = new Dictionary<RaftType, GameObject>();

    protected override void Start()
    {
        base.Start();
        foreach (RaftObjectMap item in m_RaftsInScene)
        {
            _raftMap.Add(item.Type, item.Object);
        }
    }

    public void OnRaftChange(RaftType type)
    {
        GameManager.Instance.ActiveRaft = type;
        foreach (KeyValuePair<RaftType, GameObject> item in _raftMap)
        {
            item.Value.SetActive(false);
        }

        _raftMap[type].SetActive(true);
    }
}

[System.Serializable]
public class RaftObjectMap
{
    public RaftType Type;
    public GameObject Object;
}