using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Vector2 playerInput;

    private int walkingAnimHash;
    private int runningAnimHash;
    private int jumpingAnimHash;
    private int divingAnimHash;
    
    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        walkingAnimHash = Animator.StringToHash("IsWalking");
        runningAnimHash = Animator.StringToHash("IsRunning");
        jumpingAnimHash = Animator.StringToHash("Jump");
        divingAnimHash = Animator.StringToHash("Dive");
    }

    private void Update()
    {
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        bool runPressed = Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        bool isWalkingAnim = animator.GetBool(walkingAnimHash);
        bool isRunningAnim = animator.GetBool(runningAnimHash);

        if (Input.GetKeyDown(KeyCode.LeftShift))
            animator.SetBool(runningAnimHash, true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            animator.SetBool(runningAnimHash, false);

        if (playerInput.y > 0)
        {
            if (!isWalkingAnim)
                animator.SetBool(walkingAnimHash, true);
            if (runPressed)
            {
                if (!isRunningAnim)
                    animator.SetBool(runningAnimHash, true);
            }
            else if (isRunningAnim)
                animator.SetBool(runningAnimHash, false);
        }
        else
        {
            if (isRunningAnim)
                animator.SetBool(runningAnimHash, false);
            if (isWalkingAnim)
                animator.SetBool(walkingAnimHash, false);
        }

        if (jumpPressed && !animator.GetCurrentAnimatorStateInfo(0).IsName("Jump"))
            animator.SetTrigger(jumpingAnimHash);
    }
}
