using UnityEngine;

namespace Player
{
    public class PlayerMovementTopDown : MonoBehaviour
    {
        [SerializeField] private float m_Speed = 5f;
        [SerializeField] private LayerMask m_AimLayerMask;

        [Header("AB test")]
        [SerializeField] private bool m_AimAlwaysForward = true;

        private Vector3 mPlayerMovement = Vector3.zero;
        private Animator mAnimator;
        private Rigidbody mRB;
        private Camera mMainCam;
        private Transform mCameraHolder;

        private void Awake()
        {
            mAnimator = GetComponent<Animator>();
            mRB = GetComponent<Rigidbody>();
            mMainCam = Camera.main;
            if(mMainCam!= null)
                mCameraHolder = mMainCam.transform.parent;
        }

        private void Update()
        {
            if(m_AimAlwaysForward)
                AimForward();
            else
                AimTowardsMouse();
        
            float playerInputX = Input.GetAxis("Horizontal");
            float playerInputY = Input.GetAxis("Vertical");

            bool didDive = Input.GetKeyDown(KeyCode.LeftControl);
            if (didDive && !mAnimator.GetCurrentAnimatorStateInfo(0).IsName(Constants.Animation.DIVE))
            {
                mAnimator.SetTrigger(Constants.Animation.DIVE);
            }

            mPlayerMovement = new Vector3(playerInputX, 0, playerInputY);
            float angleDiff = mCameraHolder.localRotation.eulerAngles.y;
            mPlayerMovement = Quaternion.AngleAxis(angleDiff, Vector3.up) * mPlayerMovement;

            float velocityZ = Vector3.Dot(mPlayerMovement.normalized, transform.forward);
            float velocityX = Vector3.Dot(mPlayerMovement.normalized, transform.right);
            //Debug.Log("velX:" + velocityX + ", velZ:" + velocityZ);
            mAnimator.SetFloat(Constants.Animation.VELOCITY_Z, velocityZ, .1f, Time.deltaTime);
            mAnimator.SetFloat(Constants.Animation.VELOCITY_X, velocityX, .1f, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (mPlayerMovement.magnitude > 0)
            {
                mPlayerMovement.Normalize();
                mPlayerMovement *= m_Speed * Time.deltaTime;
                mRB.MovePosition(transform.position + mPlayerMovement);
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

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
        }
    }
}
