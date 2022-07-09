using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace BaseObjects.Player
{
    public class Player : BaseObject
    {
        [Header("Player modules")]
        public PlayerMovementTopDown PlayerMovement;
        public PlayerInteraction PlayerInteraction;
        public PlayerAttack PlayerAttack;
        [Header("refs")]
        [SerializeField] internal Animator Anim;
        [SerializeField] internal Rig Rig;
        [SerializeField] internal Rigidbody Rb;
    }
}
