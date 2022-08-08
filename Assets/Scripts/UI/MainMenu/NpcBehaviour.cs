using System;
using System.Collections;
using System.Collections.Generic;
using BaseObjects.Player;
using UI;
using UnityEngine;

public class NpcBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject m_InteractionBubble;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerInteraction>() != null)
        {
            m_InteractionBubble.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerInteraction>() != null)
        {
            m_InteractionBubble.SetActive(false);
        }
    }
}
