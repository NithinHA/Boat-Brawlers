using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ResultantVectorIndicator : MonoBehaviour
{
    [SerializeField] private Transform m_ArrowTail;
    [SerializeField] private Transform m_ArrowHead;
    [SerializeField] private float m_TailScaleMultiplier = .4f;
    [SerializeField] private float m_HeadDisplacementMultiplier = .5f;
    [SerializeField] private Vector2 m_Range = new Vector2(.5f, 1f);

    private Transform _raftT;

    private void Start()
    {
        _raftT = RaftController.Instance.transform;
    }

    public void UpdateIndicator(Vector3 resultant, float raftWeightMultiplier)
    {
        float resultantMagnitude = resultant.magnitude * raftWeightMultiplier;
        // rotate towards resultant
        Quaternion rotation = Quaternion.LookRotation(resultant, Vector3.up) * Quaternion.Euler(0, -_raftT.eulerAngles.y, 0);
        transform.localRotation = rotation;

        // position arrow_head
        m_ArrowHead.localPosition = Vector3.forward * resultantMagnitude * m_HeadDisplacementMultiplier;

        // scale arrow_tail
        Vector3 localScale = m_ArrowTail.localScale;
        localScale = new Vector3(resultantMagnitude * m_TailScaleMultiplier, localScale.y, localScale.z);
        m_ArrowTail.localScale = localScale;
        
    }
}
