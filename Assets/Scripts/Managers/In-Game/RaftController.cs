using System;
using System.Collections.Generic;
using UnityEngine;
using BaseObjects;
using DG.Tweening;
using UnityEditor;
using Random = System.Random;

public class RaftController : Singleton<RaftController>
{
    [SerializeField] private float m_WeightMultiplier = .1f;
    [SerializeField] private float m_InstantneousForceMultiplier = 50f;
    [Space]
    [SerializeField] private float m_MovementThreshold = 0f;
    [SerializeField] private float m_TumbleThreshold = 30;
    [Space]
    [SerializeField] private Rigidbody m_Rb;
    [SerializeField] private Transform m_Pivot;
    [Space]
    [SerializeField] private ResultantVectorIndicator m_Indicator;
    [SerializeField] private Vector2 m_CamShakeOnGameOver = new Vector2(5f, 3f);

    public Action<BaseObject> OnObjectCreated;
    public Action<BaseObject> OnObjectDestroyed;
    
    private List<BaseObject> _allObjects = new List<BaseObject>();
    private Vector3 _resultant = Vector3.zero;

    private AudioSource _raftBgAudio;

#region Unity callbacks

    protected override void Awake()
    {
        base.Awake();
        if (m_Rb == null)
            m_Rb = GetComponent<Rigidbody>();
        m_Rb.centerOfMass = m_Pivot.localPosition;

        OnObjectCreated += OnObjectAdded;
        OnObjectDestroyed += OnObjectRemoved;
    }

    protected override void Start()
    {
        base.Start();
        AudioManager.Instance.PlaySound(Constants.SoundNames.RAFT_BG);
        _raftBgAudio = AudioManager.Instance.GetSound(Constants.SoundNames.RAFT_BG).source;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if(AudioManager.Instance != null)
            AudioManager.Instance.StopSound(Constants.SoundNames.RAFT_BG);

        OnObjectCreated -= OnObjectAdded;
        OnObjectDestroyed -= OnObjectRemoved;
    }

    /* Custom tilting
    private void Update()
    {
        if (LevelManager.Instance.IsGameEnded)
            return;

        ComputeResultantForce();
        PerformTilting();
        m_Indicator.UpdateIndicator(_resultant, m_WeightMultiplier);
        Vector3 localRot = transform.localRotation.eulerAngles;
        Vector3 localRot180 = new Vector3(Mathf.Repeat(localRot.x + 180, 360) - 180, 0, Mathf.Repeat(localRot.z + 180, 360) - 180);     // angle (-180, 180)
        UpdateRaftCrackingSound(localRot180);
        CheckForGameOver(localRot180);
    }
    */

    private void Update()
    {
        if (LevelManager.Instance.IsGameEnded)
            return;

        // if (Input.GetKeyDown(KeyCode.H))
        //     // AddInstantaneousForce(new Vector3(UnityEngine.Random.Range(-1f,1f), 0, UnityEngine.Random.Range(-1f,1f)), 3);
        //     AddInstantaneousForce(Vector3.right, 1);
        // else if(Input.GetKeyDown(KeyCode.J))
        //     AddInstantaneousForce(-Vector3.right, 1);
        
        
        ComputeResultantForce();
        m_Indicator.UpdateIndicator(_resultant, m_WeightMultiplier);
    }

    private void FixedUpdate()
    {
        Vector3 rotationVector = new Vector3(_resultant.z, 0, -_resultant.x);     // If the resultant vector is on X axis-rotate along Z axis; resultant vector is on Z axis-rotate along X axis
        m_Rb.AddTorque(rotationVector * m_WeightMultiplier);
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
        rotationVector = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.down) * rotationVector;     // Rotate the resultantVector by this Raft's Y rotation. This makes sure tilting happens properly even if Raft has some non-zero Y rotation.

        if (_resultant.magnitude > m_MovementThreshold)
        {
            m_Rb.rotation = Quaternion.Euler(m_Rb.rotation.eulerAngles + rotationVector * m_WeightMultiplier * Time.deltaTime);
        }
    }

    private void UpdateRaftCrackingSound(Vector3 localRot180)
    {
        float diff = Mathf.Min(Mathf.Abs(m_TumbleThreshold) - Mathf.Abs(localRot180.x), Mathf.Abs(m_TumbleThreshold) - Mathf.Abs(localRot180.z));
        float amount = 1 - diff / m_TumbleThreshold;        // [0, 1]
        if(_raftBgAudio != null)
            _raftBgAudio.volume = amount * .8f;

    }

    private void CheckForGameOver(Vector3 localRot180)
    {
        // Vector3 localRot = transform.localRotation.eulerAngles;
        // if ((localRot.x > m_TumbleThreshold.x && localRot.x < m_TumbleThreshold.y) || (localRot.z > m_TumbleThreshold.x && localRot.z < m_TumbleThreshold.y))
        if(Mathf.Abs(localRot180.x) > m_TumbleThreshold || Mathf.Abs(localRot180.z) > m_TumbleThreshold)
        {
            m_Rb.isKinematic = false;
            Debug.Log("DIEDED! (" + localRot180 + ")");
            CameraShake.ShakeOnce(m_CamShakeOnGameOver.x, m_CamShakeOnGameOver.y);
            AudioManager.Instance.StopSound(Constants.SoundNames.RAFT_BG);
            AudioManager.Instance.PlaySound(Constants.SoundNames.RAFT_FALL);
            DOVirtual.DelayedCall(1f, () => AudioManager.Instance.PlaySound(Constants.SoundNames.RAFT_SINK));
            LevelManager.Instance.GameLost();
        }
    }

    private Vector3 _itemPos = Vector3.zero;
    /// <summary>
    /// Work in progress
    /// </summary>
    public void AddInstantaneousForce(Vector3 itemPos, float weight)
    {
        _itemPos = itemPos;
        Vector3 force = (itemPos - m_Pivot.position) * weight * m_InstantneousForceMultiplier;
        force.y = 0;
        // force.y = 0;
        // Vector3 rotationVector = new Vector3(force.z, 0, -force.x);
        // rotationVector = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.down) * rotationVector;
        // m_Rb.rotation = Quaternion.Euler(m_Rb.rotation.eulerAngles + rotationVector * m_WeightMultiplier * Time.deltaTime);

        // Vector3 torqueAxis = Vector3.Cross(force.normalized, Vector3.up);
        Vector3 rotationVector = new Vector3(force.z, 0, -force.x);     // If the resultant vector is on X axis-rotate along Z axis; resultant vector is on Z axis-rotate along X axis
        m_Rb.AddTorque(rotationVector, ForceMode.Impulse);
        // m_Rb.AddTorque(torqueAxis * force.magnitude * m_WeightMultiplier);
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
        
        Handles.color = Color.green;
        Handles.DrawDottedLine(_itemPos, m_Pivot.position, 3);
    }
}
