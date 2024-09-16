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

        public static Player Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
            {
                Debug.LogError("Multiple instances of Player exists!");
                Destroy(this.gameObject);
            }

            GameManager.Instance.PlayerMovementToggle += OnPlayerMovementToggle;
        }

        private void OnPlayerMovementToggle(bool active)
        {
            PlayerMovement.IsMovementEnabled = active;
        }
    }
}
