using System;
using UI;
using UnityEngine;

namespace NPC
{
    public class RaftSelectionNpcBehaviour : BaseNpcBehaviour
    {
        [SerializeField] private Animator m_Animator;

        private const string InteractionKey = "Interact";
        private static readonly int Interact = Animator.StringToHash(InteractionKey);

        private void Start()
        {
            m_TargetCamera = MainMenuController.Instance.RaftSelectionCam;
        }

        protected override void OnInteract()
        {
            base.OnInteract();
            m_Animator.SetBool(Interact, true);
            MainMenuUI.Instance.RaftSelectionScreen.Show(OnReset);
        }

        protected override void OnReset()
        {
            base.OnReset();
            m_Animator.SetBool(Interact, false);
        }
    }
}