using System;
using System.Collections.Generic;
using UnityEngine;
using BaseObjects;
using DG.Tweening;
using UnityEditor;

public class RaftController_Custom : Singleton<RaftController_Custom>
{
    [SerializeField] private float m_WeightMultiplier = .1f;
    [SerializeField] private float m_InstantneousForceMultiplier = 50f;
    [SerializeField] private float m_ResultantMaxMagnitude = 5f;        // if the resultant force on the Raft is greater than this number, it gets clamped to this value.
    [Space]
    [SerializeField] private float m_MovementThreshold = 0f;
    [SerializeField] private float m_TumbleThreshold = 30;
    [Space]
    [SerializeField] private Rigidbody m_Rb;
    [SerializeField] private Transform m_Pivot;
    [SerializeField] private BoxCollider m_PlatformBounds;
    [Space]
    [SerializeField] private ResultantVectorIndicator m_Indicator;
    [SerializeField] private Vector2 m_CamShakeOnGameOver = new Vector2(5f, 3f);

    public Action<BaseObject> OnObjectCreated;
    public Action<BaseObject> OnObjectDestroyed;
    
    private List<BaseObject> _allObjects = new List<BaseObject>();
    private Vector3 _resultant = Vector3.zero;

    private Vector3 _instantaneousTorque = Vector3.zero;
    private Tween _instantaneousTorqueCooldownTween;

    private Vector3 _localTilt180;       // holds the current raft tilt [-180, 180]
    private Vector3? _platformBounds = null;

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
        _localTilt180 = new Vector3(Mathf.Repeat(localRot.x + 180, 360) - 180, 0, Mathf.Repeat(localRot.z + 180, 360) - 180);     // angle (-180, 180)
        UpdateRaftCrackingSound();
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

        if (_resultant.magnitude > m_ResultantMaxMagnitude)      // perform clamping
            _resultant = Vector3.Normalize(_resultant) * m_ResultantMaxMagnitude;
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

    private void UpdateRaftCrackingSound()
    {
        float amount = GetTiltPercent01();
        if(_raftBgAudio != null)
            _raftBgAudio.volume = amount * .8f;

    }

    private void CheckForGameOver()
    {
        float tiltAmount = GetTiltPercent01();
        if (tiltAmount >= 1)
        {
            if(_instantaneousTorqueCooldownTween != null)
                _instantaneousTorqueCooldownTween.Kill();

            m_Rb.isKinematic = false;
            Debug.Log("DIEDED! (" + tiltAmount + ")");
            CameraHolder.Instance.TriggerCameraShake(.8f, 1, .2f);
            CameraHolder.Instance.PlayerCam.Follow = null;
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

    /// <summary>
    /// Returns value between 0-1; 0 being completely stable; 1 being the point where the raft tumbles and falls.
    /// </summary>
    public float GetTiltPercent01()
    {
        float tiltValue = Mathf.Max(Mathf.Abs(_localTilt180.x), Mathf.Abs(_localTilt180.z));
        return Mathf.Clamp01(tiltValue / m_TumbleThreshold);
    }

    /// <summary>
    /// Returns the platform boundary using a dummy collider.
    /// </summary>
    public Vector3 GetPlatformBounds()
    {
        _platformBounds ??= m_PlatformBounds.size * 0.5f;
        return _platformBounds.Value;
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


#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(m_Pivot.position, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_resultant, .1f);

        Handles.color = Color.cyan;
        Handles.DrawDottedLine(new Vector3(-_instantaneousTorque.z, 0, _instantaneousTorque.x) * .1f, m_Pivot.position, 2);
    }
    
#endif
}
