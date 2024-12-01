using UnityEngine;

public class ResultantVectorIndicator : MonoBehaviour
{
    [SerializeField] private LineRenderer m_Line;
    [SerializeField] private float m_MinThreshold = 1;  // if resultant is below this value, don't show indicator.
    [Space]
    [SerializeField] private float m_ResultantMagnitudeMultiplier = 1.5f;
    [SerializeField] private Transform m_RaftT;

    Vector3[] _linePositions;
    Vector3[] _defaultLinePositions = new Vector3[] { Vector3.zero, Vector3.zero };

    private void Awake()
    {
        m_Line.positionCount = 2;
    }

    public void UpdateIndicator(Vector3 resultant, float raftWeightMultiplier)
    {
        float resultantMagnitude = resultant.magnitude * raftWeightMultiplier;
        // rotate towards resultant
        Quaternion rotation = Quaternion.LookRotation(resultant, Vector3.up) * Quaternion.Euler(0, -m_RaftT.eulerAngles.y, 0);
        transform.localRotation = rotation;

        // set LineRenderer positions
        Vector3 startPos = transform.position;
        Vector3 endPos = Vector3.forward * resultantMagnitude * m_ResultantMagnitudeMultiplier;

        _linePositions = _defaultLinePositions;

        if (resultant.magnitude > m_MinThreshold)
            _linePositions = new Vector3[] { startPos, endPos };

        m_Line.SetPositions(_linePositions);
    }
}
