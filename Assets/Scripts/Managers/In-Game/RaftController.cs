using System;
using System.Collections.Generic;
using UnityEngine;
using BaseObjects;

public class RaftController : Singleton<RaftController>
{
    [SerializeField] private float m_WeightMultiplier = .1f;
    [SerializeField] private float m_MovementThreshold = 0f;
    [SerializeField] private Vector2 m_TumbleThreshold = new Vector2(30, 330);
    [SerializeField] private Rigidbody m_Rb;
    [SerializeField] private Transform m_Pivot;
    
    public Action<BaseObject> OnObjectCreated;
    public Action<BaseObject> OnObjectDestroyed;
    
    private List<BaseObject> _allObjects = new List<BaseObject>();
    private Vector3 _resultant = Vector3.zero;

#region Unity callbacks

    protected override void Awake()
    {
        base.Awake();
        if (m_Rb == null)
            m_Rb = GetComponent<Rigidbody>();

        OnObjectCreated += OnObjectAdded;
        OnObjectDestroyed += OnObjectRemoved;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnObjectCreated -= OnObjectAdded;
        OnObjectDestroyed -= OnObjectRemoved;
    }

    private void Update()
    {
        if (LevelManager.Instance.IsGameEnded)
            return;

        ComputeResultantForce();
        PerformTilting();
        CheckForGameOver();
    }

#endregion

    private void ComputeResultantForce()
    {
        _resultant = Vector3.zero;
        foreach (BaseObject item in _allObjects)
        {
            Vector3 weight = (item.transform.position - m_Pivot.position) * item.Weight;
            weight.y = 0;
            _resultant += weight;
        }
    }

    private void PerformTilting()
    {
        Vector3 rotationVector = new Vector3(_resultant.z, 0, -_resultant.x);     // If the resultant vector is on X axis-rotate along Z axis; resultant vector is on Z axis-rotate along X axis
        rotationVector = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.down) * rotationVector;     // Rotate the resultantVector by this Raft's Y rotation 

        if (_resultant.magnitude > m_MovementThreshold)
        {
            m_Rb.rotation = Quaternion.Euler(m_Rb.rotation.eulerAngles + rotationVector * m_WeightMultiplier * Time.deltaTime);
        }
    }

    private void CheckForGameOver()
    {
        Vector3 localRot = transform.localRotation.eulerAngles;
        if ((localRot.x > m_TumbleThreshold.x && localRot.x < m_TumbleThreshold.y) || (localRot.z > m_TumbleThreshold.x && localRot.z < m_TumbleThreshold.y))
        {
            m_Rb.isKinematic = false;
            Debug.LogError("DIEDED! (" + localRot + ")");
            LevelManager.Instance.GameLost();
            // Disable player control
            // End game
        }
    }

    public void AddInstantaneousForce(Vector3 itemPos, float weight)
    {
        Vector3 force = (itemPos - m_Pivot.position) * weight;
        force.y = 0;
        Vector3 rotationVector = new Vector3(force.z, 0, -force.x);
        rotationVector = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.down) * rotationVector;
        m_Rb.rotation = Quaternion.Euler(m_Rb.rotation.eulerAngles + rotationVector * m_WeightMultiplier * Time.deltaTime);

    }

#region Event listeners

    private void OnObjectAdded(BaseObject item)
    {
        _allObjects.Add(item);
    }

    private void OnObjectRemoved(BaseObject item)
    {
        _allObjects.Remove(item);
    }

#endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(m_Pivot.position, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_resultant, .1f);
    }
}
