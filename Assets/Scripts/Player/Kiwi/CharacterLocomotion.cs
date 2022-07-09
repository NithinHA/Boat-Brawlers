using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLocomotion : MonoBehaviour
{
    public enum MovementState
    {
        Standard, LandRoll, Dash, PostDash
    }
    public MovementState currentState = MovementState.Standard;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float airControl = 4f;
    [Space]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float stepDown = .3f;
    [Space]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDamp = .5f;
    [Space]
    [SerializeField] private float pushPower = 5f;
    [SerializeField] private float airTimeThreshold = 1.5f;
    [Space]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform feetT;
    [Space]
    private Vector3 rootMotion;
    private bool lockInput;
    
    private Vector3 moveVelocity;
    //private Vector3 dashVelocity;
    private bool didSprintBeforeJump;
    private bool isJumping;
    private float airTime;

    private bool isSprinting;
    private bool wasSprintingLastFrame;
    private Vector3 groundedFeetBoxHalfExtent = new Vector3(.15f, .1f, .15f);
    private bool isGrounded
    {
        get
        {
            //var groundColliders = Physics.OverlapSphere(feetT.position, .1f, groundLayer);
            var groundColliders = Physics.OverlapBox(feetT.position, groundedFeetBoxHalfExtent, feetT.rotation, groundLayer);
            return groundColliders.Length > 0;
        }
    }

    private Transform mainCam;
    private Animator animator;
    private Vector3 playerInput;
    private CharacterController characterController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        mainCam = Camera.main.transform;
    }

    private void Update()
    {
        if (lockInput)
            return;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.z = Input.GetAxis("Vertical");
        playerInput.Normalize();

        isSprinting = Input.GetKey(KeyCode.LeftShift) && playerInput.magnitude > 0;

        animator.SetFloat("VelocityZ", playerInput.z, .01f, Time.deltaTime);
        animator.SetFloat("VelocityX", playerInput.x, .01f, Time.deltaTime);

        if (!wasSprintingLastFrame && isSprinting)
        {
            wasSprintingLastFrame = true;
            animator.SetBool("IsSprinting", isSprinting);
        }
        else if (wasSprintingLastFrame && !isSprinting)
        {
            wasSprintingLastFrame = false;
            animator.SetBool("IsSprinting", isSprinting);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl))
            Dash();
    }

    private void OnAnimatorMove()
    {
        rootMotion += animator.deltaPosition;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }

    /*
    private void _FixedUpdate()
    {
        AimTowardsMouse();
        Vector3 moveBy = mainCam.transform.right * playerInput.x + mainCam.transform.forward * playerInput.y;
        moveBy.Normalize();
        if (isJumping)
        {
            moveVelocity.y -= gravity * Time.fixedDeltaTime;
            characterController.Move(moveVelocity * Time.fixedDeltaTime);
            isJumping = !isGrounded;
            animator.SetBool("IsMidAir", isJumping);
            rootMotion = Vector3.zero;
            moveBy *= airControl;
        }
        else
        {
            playerInput.Normalize();
            characterController.Move(rootMotion);
            if (!isGrounded)
            {
                isJumping = true;
                animator.SetBool("IsMidAir", isJumping);
                moveVelocity = animator.velocity * jumpDamp;
                moveVelocity.y = 0;
                moveBy *= airControl;
            }
            else
            {
                characterController.Move(Vector3.down * stepDown);
                moveBy *= moveSpeed;
            }
        }
        moveBy.y = 0;
        characterController.Move(moveBy * .025f);
    }

    private void __FixedUpdate()
    {
        AimTowardsMouse();
        if (isJumping)
        {
            moveVelocity.y -= gravity * Time.fixedDeltaTime;
            Vector3 displacement = moveVelocity * Time.fixedDeltaTime;
            displacement += CalculateAirControl();
            characterController.Move(displacement);
            isJumping = !isGrounded;
            rootMotion = Vector3.zero;
            animator.SetBool("IsMidAir", isJumping);
        }
        else
        {
            Vector3 stepForwardAmount = rootMotion;
            Vector3 stepDownAmount = Vector3.down * stepDown;

            characterController.Move(stepForwardAmount + stepDownAmount);
            rootMotion = Vector3.zero;

            if (!isGrounded)
            {
                isJumping = true;
                moveVelocity = animator.velocity * jumpDamp * moveSpeed;
                moveVelocity.y = 0;
                animator.SetBool("IsMidAir", true);
            }
        }
    }
    */

    private void FixedUpdate()
    {
        AimTowardsMouse();
        if(currentState == MovementState.Dash || !isJumping)
        {
            UpdateOnGround();
            // add the complete dashing logic here
            //var dashMove = ((mainCam.forward * playerInput.z) + (mainCam.right * playerInput.x)).normalized;
            //characterController.Move(dashMove * 30 * Time.fixedDeltaTime);
        }
        //else if (isJumping)
        else
            UpdateInAir();
            //UpdateOnGround();
    }

    private void UpdateInAir()
    {
        moveVelocity.y -= gravity * Time.fixedDeltaTime;
        Vector3 displacement = moveVelocity * Time.fixedDeltaTime;
        var airControl = CalculateAirControl();
        airControl.y = 0;
        displacement += airControl;
        characterController.Move(displacement);
        airTime += Time.deltaTime;
        isJumping = !isGrounded;
        rootMotion = Vector3.zero;
        animator.SetBool("IsMidAir", isJumping);
        if (isGrounded && airTime > airTimeThreshold)
        {
            StartCoroutine(FallToRoll());
        }
    }

    private void UpdateOnGround()
    {
        Vector3 stepForwardAmount = rootMotion * moveSpeed;
        Vector3 stepDownAmount = Vector3.down * stepDown;
        Vector3 moveBy = Vector3.zero;

        switch (currentState)
        {
            case MovementState.Standard:
                moveBy = ((mainCam.forward * playerInput.z) + (mainCam.right * playerInput.x)).normalized * (isSprinting ? 2 : 1);
                break;
            case MovementState.LandRoll:
                moveBy = transform.forward;
                break;
            case MovementState.Dash:
                moveBy = ((mainCam.forward * playerInput.z) + (mainCam.right * playerInput.x)).normalized;
                characterController.Move(moveBy * 30 * Time.fixedDeltaTime);
                return;
            case MovementState.PostDash:
                return;
        }

        characterController.Move(stepForwardAmount + stepDownAmount + moveBy * Time.fixedDeltaTime * moveSpeed);
        rootMotion = Vector3.zero;
        if (!isGrounded)
            SetInAir(0);
    }

    private Vector3 CalculateAirControl()
    {
        Vector3 result = Vector3.zero;
        switch (currentState)
        {
            case MovementState.Standard:
                result = ((mainCam.forward * playerInput.z) + (mainCam.right * playerInput.x)).normalized * (airControl / 100) * (didSprintBeforeJump ? 2 : 1);
                break;
            case MovementState.LandRoll:
                result = transform.forward * (airControl / 100);
                break;
            //case MovementState.Dash:
            //    result = transform.forward * (airControl / 100) + dashVelocity;
            //    break;
        }
        
        return result;
    }

    private void AimTowardsMouse()
    {
        //bool isDashing = false;
        switch (currentState)
        {
            case MovementState.Standard:
                break;
            case MovementState.LandRoll:
                return;
            case MovementState.Dash:
                float angle = Mathf.Atan2(playerInput.x, playerInput.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), 1f);
                return;
            //case MovementState.PostDash:
            //    return;
        }

        if (!isSprinting && !isJumping)
        {
            float cameraY = mainCam.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, cameraY, 0), .1f);
        }
        else
        {
            float angle = Mathf.Atan2(playerInput.x, playerInput.z) * Mathf.Rad2Deg + mainCam.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, angle, 0), .1f);
        }
        // float angleDiff = (cameraY - transform.rotation.eulerAngles.y);
        // animator.SetFloat("RotAngle", angleDiff > 20 ? 1 : angleDiff < -20 ?  -1 : 0);
    }

    private void Jump()
    {
        switch (currentState)
        {
            case MovementState.Standard:
                break;
            case MovementState.LandRoll:
                return;
            case MovementState.Dash:
                return;
            case MovementState.PostDash:
                return;
        }

        if (!isJumping)
        {
            float jumpVelocity = Mathf.Sqrt(2 * gravity * jumpHeight);
            didSprintBeforeJump = isSprinting;
            animator.Play("Leap");
            
            SetInAir(jumpVelocity, false);
        }
    }

    private void SetInAir(float jumpVelocity, bool didJump = false)
    {
        isJumping = true;
        airTime = 0;
        //moveVelocity = animator.velocity + mainCam.forward * jumpDamp * moveSpeed * (isSprinting ? 2 : 1);
        moveVelocity.y = jumpVelocity;
        if(!didJump)
            animator.SetBool("IsMidAir", true);
    }

    private void Dash()
    {
        switch (currentState)
        {
            case MovementState.Standard:
                break;
            case MovementState.LandRoll:
                return;
            case MovementState.Dash:
                return;
            case MovementState.PostDash:
                return;
        }

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        lockInput = true;
        currentState = MovementState.Dash;
        animator.Play("Sprinting Forward Roll");
        yield return new WaitForSeconds(.3f);
        currentState = MovementState.PostDash;
        yield return new WaitForSeconds(.8f);
        currentState = MovementState.Standard;
        lockInput = false;
    }

    private IEnumerator FallToRoll()
    {
        lockInput = true;
        currentState = MovementState.LandRoll;
        animator.Play("Falling To Roll");
        yield return new WaitForSeconds(1.2f);
        currentState = MovementState.Standard;
        lockInput = false;
    }

    private void OnDrawGizmos()
    {
        if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(feetT.position, .1f);
        Gizmos.DrawCube(feetT.position, groundedFeetBoxHalfExtent * 2);
    }
}
