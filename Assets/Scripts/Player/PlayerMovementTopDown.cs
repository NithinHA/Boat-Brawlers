using System;
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

        [Header("AB test")]
        [SerializeField] private bool m_AimAlwaysForward = true;

        private Vector3 mPlayerMovement = Vector3.zero;
        private Camera mMainCam;
        private Transform mCameraHolder;
        [SerializeField] private float _curSpeed;

        public bool IsMovementEnabled = true;

        private void Awake()
        {
            mMainCam = Camera.main;
            if (mMainCam != null)
                mCameraHolder = mMainCam.transform.parent;
            if (m_FloatingJoystick == null)
                m_FloatingJoystick = FindObjectOfType<Joystick>();

            _curSpeed = m_MaxSpeed;

            m_Player.PlayerInteraction.OnItemPicked += OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped += OnItemDropped;
            LevelManager.Instance.OnGameEnd += OnGameEnd;
        }

        private void OnDestroy()
        {
            m_Player.PlayerInteraction.OnItemPicked -= OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped -= OnItemDropped;
            if(LevelManager.Instance != null)
                LevelManager.Instance.OnGameEnd -= OnGameEnd;
        }

        private void Update()
        {
            if (!IsMovementEnabled)
                return;

            if(m_AimAlwaysForward)
                AimForward();
            else
                AimTowardsMouse();
        
#if UNITY_EDITOR
            float playerInputX = Input.GetAxis("Horizontal");
            float playerInputY = Input.GetAxis("Vertical");
#else
            float playerInputX = m_FloatingJoystick.Horizontal;
            float playerInputY = m_FloatingJoystick.Vertical;
#endif

            // bool didDive = Input.GetKeyDown(KeyCode.LeftControl);
            // if (didDive && !m_Player.Anim.GetCurrentAnimatorStateInfo(0).IsName(Constants.Animation.DIVE))
            // {
            //     m_Player.Anim.SetTrigger(Constants.Animation.DIVE);
            // }

            mPlayerMovement = new Vector3(playerInputX, 0, playerInputY);
            float angleDiff = mCameraHolder.localRotation.eulerAngles.y;
            mPlayerMovement = Quaternion.AngleAxis(angleDiff, Vector3.up) * mPlayerMovement;

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
                m_Player.Rb.MovePosition(transform.position + mPlayerMovement);
            }
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
            m_Player.Anim.SetTrigger(Constants.Animation.VICTORY);
        }

#endregion

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
        }
    }
}
