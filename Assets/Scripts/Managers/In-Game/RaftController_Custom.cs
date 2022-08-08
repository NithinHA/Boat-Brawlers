using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseObjects;
using DG.Tweening;
using UnityEditor;
using Random = System.Random;

public class RaftController_Custom : Singleton<RaftController_Custom>
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

    private Vector3 _instantaneousTorque = Vector3.zero;
    private Tween _instantaneousTorqueCooldownTween;

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
        rotationVector += _instantaneousTorque;
        rotationVector = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.down) * rotationVector;     // Rotate the resultantVector by this Raft's Y rotation. This makes sure tilting happens properly even if Raft has some non-zero Y rotation.

        if (rotationVector.magnitude > m_MovementThreshold)
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
        if(Mathf.Abs(localRot180.x) > m_TumbleThreshold || Mathf.Abs(localRot180.z) > m_TumbleThreshold)
        {
            if(_instantaneousTorqueCooldownTween != null)
                _instantaneousTorqueCooldownTween.Kill();

            m_Rb.isKinematic = false;
            Debug.Log("DIEDED! (" + localRot180 + ")");
            CameraShake.ShakeOnce(m_CamShakeOnGameOver.x, m_CamShakeOnGameOver.y);
            AudioManager.Instance.StopSound(Constants.SoundNames.RAFT_BG);
            AudioManager.Instance.PlaySound(Constants.SoundNames.RAFT_FALL);
            DOVirtual.DelayedCall(1f, () => AudioManager.Instance.PlaySound(Constants.SoundNames.RAFT_SINK));
            LevelManager.Instance.GameLost();
        }
    }

    /// <summary>
    /// Calculate instantaneous Torque applied on the Raft from given hitPoint and weight.
    /// Tween the instantaneous Torque from calculated value to 0 over next few frames. Initial bump and slow fade out gives the best result, ie.- Ease.OutExpo.
    /// </summary>
    public void AddInstantaneousForce(Vector3 hitPoint, float weight)
    {
        if(_instantaneousTorqueCooldownTween != null)
            _instantaneousTorqueCooldownTween.Kill();

        Vector3 hitForce = (hitPoint - m_Pivot.position) * weight * m_InstantneousForceMultiplier;
        hitForce.y = 0;
        _instantaneousTorque = new Vector3(hitForce.z, 0, -hitForce.x);     // If the resultant vector is on X axis- Rotate along Z axis; resultant vector is on Z axis- Rotate along X axis
        _instantaneousTorqueCooldownTween = DOTween.To(() => _instantaneousTorque, x => _instantaneousTorque = x, Vector3.zero, 1.5f).SetEase(Ease.OutExpo);
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

        Handles.color = Color.cyan;
        Handles.DrawDottedLine(new Vector3(-_instantaneousTorque.z, 0, _instantaneousTorque.x) * .1f, m_Pivot.position, 2);
    }
}
