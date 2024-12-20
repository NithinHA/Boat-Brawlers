﻿using System;
using UnityEngine;

namespace BaseObjects.Player
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovementTopDown : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [Space]
        [SerializeField] private float m_MaxSpeed = 5f;
        [SerializeField] private LayerMask m_AimLayerMask;
        [SerializeField] private Joystick m_FloatingJoystick;
        [Space]
        [SerializeField] private ParticleSystem m_RunParticles;
        [SerializeField] private bool _isPlayerMoving = false;

        [Header("AB test")]
        [SerializeField] private bool m_AimAlwaysForward = true;

        private Vector3 mPlayerMovement = Vector3.zero;
        private Camera mMainCam;
        [SerializeField] private float _curSpeed;

        public bool IsMovementEnabled = true;

        private void Awake()
        {
            mMainCam = Camera.main;
            if (m_FloatingJoystick == null)
                m_FloatingJoystick = FindObjectOfType<Joystick>();

            _curSpeed = m_MaxSpeed;

            m_Player.PlayerInteraction.OnItemPicked += OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped += OnItemDropped;
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnGameEnd += OnGameEnd;
        }

        private void OnDestroy()
        {
            m_Player.PlayerInteraction.OnItemPicked -= OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped -= OnItemDropped;
            if (LevelManager.Instance != null)
                LevelManager.Instance.OnGameEnd -= OnGameEnd;
        }

        private void Update()
        {
            if (!IsMovementEnabled)
            {
                if (_isPlayerMoving)
                    TogglePlayerMovementBoolean(false);
                return;
            }

            if(m_AimAlwaysForward)
                AimForward();
            else
                AimTowardsMouse();

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            float playerInputX = m_FloatingJoystick.Horizontal;
            float playerInputY = m_FloatingJoystick.Vertical;
#else
            float playerInputX = Input.GetAxis("Horizontal");
            float playerInputY = Input.GetAxis("Vertical");
#endif

            // bool didDive = Input.GetKeyDown(KeyCode.LeftControl);
            // if (didDive && !m_Player.Anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.Animation.DIVE))
            // {
            //     m_Player.Anim.SetTrigger(Constants.Animation.DIVE);
            // }

            mPlayerMovement = new Vector3(playerInputX, 0, playerInputY);
            // float angleDiff = 0;
            // mPlayerMovement = Quaternion.AngleAxis(angleDiff, Vector3.up) * mPlayerMovement;     // This handles input correction if the camera has a certain initial Y rotation.

            float velocityZ = Vector3.Dot(mPlayerMovement.normalized, transform.forward);
            float velocityX = Vector3.Dot(mPlayerMovement.normalized, transform.right);
            //Debug.Log("velX:" + velocityX + ", velZ:" + velocityZ);
            m_Player.Anim.SetFloat(Constants.Animation.VELOCITY_Z, velocityZ, .1f, Time.deltaTime);
            m_Player.Anim.SetFloat(Constants.Animation.VELOCITY_X, velocityX, .1f, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!IsMovementEnabled)
                return;

            if (mPlayerMovement.magnitude > 0)
            {
                mPlayerMovement.Normalize();
                mPlayerMovement *= _curSpeed * Time.deltaTime;

                MoveAsPerConstraints();

                if (!_isPlayerMoving)
                    TogglePlayerMovementBoolean(true);
            }
            else
            {
                if (_isPlayerMoving)
                    TogglePlayerMovementBoolean(false);
            }
        }

        private void MoveAsPerConstraints()
        {
            Vector3 currentPos = transform.position;
            Vector3 destination = currentPos + mPlayerMovement;
            if(RaftController.Instance == null)
            {
                m_Player.Rb.MovePosition(destination);
                return;
            }

            Transform raftTransform = RaftController_Custom.Instance.transform;
            Vector3 localPos = raftTransform.InverseTransformPoint(destination);    // gives positions with respect to the raft space.
            Vector3 raftBounds = RaftController_Custom.Instance.GetPlatformBounds();

            bool exceedX = Mathf.Abs(localPos.x) > Mathf.Abs(raftBounds.x);         // whether the player's next position crosses the raft boundaries on X axis.
            bool exceedZ = Mathf.Abs(localPos.z) > Mathf.Abs(raftBounds.z);         // whether the player's next position crosses the raft boundaries on Y axis.

            if (exceedX && exceedZ)     // if exceed on both axes, then make no movement.
            {
                return;
            }

            if (exceedX)
            {
                localPos.x = Mathf.Clamp(localPos.x, -raftBounds.x, raftBounds.x);
                destination = raftTransform.TransformPoint(localPos);           // conversion back from raft space to world space.
                destination.z = currentPos.z + mPlayerMovement.z;       // slide along the Z axis without making any movement on X axis.
            }
            else if (exceedZ)
            {
                localPos.z = Mathf.Clamp(localPos.z, -raftBounds.z, raftBounds.z);
                destination = raftTransform.TransformPoint(localPos);           // conversion back from raft space to world space.
                destination.x = currentPos.x + mPlayerMovement.x;       // slide along the X axis without making any movement on Z axis.
            }

            m_Player.Rb.MovePosition(destination);
        }

        private void AimTowardsMouse()
        {
            Ray ray = mMainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, m_AimLayerMask))
            {
                Vector3 direction = hitInfo.point - transform.position;
                direction.y = 0;
                direction.Normalize();
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), .1f);
            }
        }

        private void AimForward()
        {
            if (mPlayerMovement.magnitude > 0)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(mPlayerMovement.normalized), .1f);
            }
        }

        private void TogglePlayerMovementBoolean(bool active)
        {
            m_Player.Anim.SetFloat(Constants.Animation.VELOCITY_Z, 0, .1f, Time.deltaTime);
            m_Player.Anim.SetFloat(Constants.Animation.VELOCITY_X, 0, .1f, Time.deltaTime);
            _isPlayerMoving = active;
            if (active)
                m_RunParticles.Play();
            else
                m_RunParticles.Stop();
        }

#region Event listeners

        /// <summary>
        /// diff follows the curve with points: 1, 1.75, 2.25, 2.5; corresponding to weights
        /// Subsequent difference after each weight is decreased by 0.25; ie- Sequence: 1, 0.75, 0.5, 0.25 
        /// </summary>
        private void OnItemPicked(InteractableObject item)
        {
            float weight = item.Weight;
            Mathf.Clamp(weight, 0, 4);
            float diff = 1.125f * item.Weight - 0.125f * Mathf.Pow(weight, 2);      // difference based on the polynomial equation
            _curSpeed = m_MaxSpeed - diff;
        }

        private void OnItemDropped(InteractableObject item)
        {
            _curSpeed = m_MaxSpeed;
        }

        private void OnGameEnd(bool isWin)
        {
            IsMovementEnabled = false;
            m_Player.PlayerInteraction.DropItem();
            if (isWin)
                m_Player.Anim.SetTrigger(Constants.Animation.VICTORY);
        }

#endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
        }
    }
}
