using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : Singleton<MainMenuController>
{
    [SerializeField] private GameObject[] m_RaftsInScene;
    public int ActiveRaft = 1;

    public void OnRaftChange(int index)
    {
        ActiveRaft = index;
        foreach (GameObject raft in m_RaftsInScene)
        {
            raft.SetActive(false);
        }

        m_RaftsInScene[index - 1].SetActive(true);
    }
}
