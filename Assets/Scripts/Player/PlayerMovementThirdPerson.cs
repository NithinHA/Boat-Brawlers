using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementThirdPerson : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private float m_TurnSpeed = 5f;
    [SerializeField] private LayerMask m_AimLayerMask;

    private Vector3 mPlayerMovement;
    private Animator mAnimator;
    private Rigidbody mRB;
    private Camera mMainCam;

    private void Awake()
    {
        mAnimator = GetComponent<Animator>();
        mRB = GetComponent<Rigidbody>();
        mMainCam = Camera.main;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float playerInputX = Input.GetAxis("Horizontal");
        float playerInputZ = Input.GetAxis("Vertical");
        mPlayerMovement = new Vector3(playerInputX, 0, playerInputZ).normalized;

        //bool didDive = Input.GetKeyDown(KeyCode.LeftControl);
        //if (didDive && !mAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dive"))
        //{
        //    mAnimator.SetTrigger("Dive");
        //}


        mAnimator.SetFloat("VelocityZ", mPlayerMovement.z, .1f, Time.deltaTime);
        mAnimator.SetFloat("VelocityX", mPlayerMovement.x, .1f, Time.deltaTime);
    }

    Vector3 tempMoveDir;
    private void FixedUpdate()
    {
        AimTowardsMouse();
        if (mPlayerMovement.magnitude > 0.1f)
        {
            mPlayerMovement.Normalize();
            //mPlayerMovement *= m_Speed * Time.deltaTime;
            float targetAngle = Mathf.Atan2(mPlayerMovement.x, mPlayerMovement.z) * Mathf.Rad2Deg * mMainCam.transform.eulerAngles.y;
            //Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            //tempMoveDir = moveDir;
            //mRB.MovePosition(transform.position + moveDir * m_Speed * Time.fixedDeltaTime);

            Vector3 moveBy = mMainCam.transform.right * mPlayerMovement.x + mMainCam.transform.forward * mPlayerMovement.z;
            mRB.MovePosition(transform.position + moveBy.normalized * m_Speed * Time.fixedDeltaTime);
        }
    }

    private void AimTowardsMouse()
    {
        float cameraY = mMainCam.transform.rotation.eulerAngles.y;
        //float targetAngle = Mathf.Atan2(mPlayerMovement.x, mPlayerMovement.z) * Mathf.Rad2Deg * cameraY;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, cameraY, 0), .1f);
        //float angle = transform.rotation.eulerAngles.y - cameraY;
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, cameraY, 0), m_TurnSpeed * Time.deltaTime);
        //Debug.Log(angle);
        //mAnimator.SetFloat("RotAngle", angle, .1f, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + tempMoveDir.normalized * 100);
    }
}
