using BaseObjects.Player;
using Cinemachine;
using UI;
using UnityEngine;

namespace NPC
{
    public class BaseNpcBehaviour : MonoBehaviour
    {
        [SerializeField] private float m_BubbleOffsetY = 3f;
        [SerializeField] protected CinemachineVirtualCamera m_TargetCamera;

        /// <summary>
        /// Hide InteractionBubble
        /// Switch camera state
        /// Toggle PLayerMovement
        /// </summary>
        protected virtual void OnInteract()
        {
            MainMenuUI.Instance.InteractionBubblesController.HideInteractionBubble(transform);
            if (m_TargetCamera != null)
                MenuCameraSwitcher.SwitchCamera(m_TargetCamera);
            GameManager.Instance.PlayerMovementToggle?.Invoke(false);
        }

        protected virtual void OnReset()
        {
            MenuCameraSwitcher.SwitchCamera(MainMenuController.Instance.PlayerCam);
            GameManager.Instance.PlayerMovementToggle?.Invoke(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerInteraction>() != null)
            {
                MainMenuUI.Instance.InteractionBubblesController.ShowInteractionBubble(transform, Vector3.up * m_BubbleOffsetY, OnInteract);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerInteraction>() != null)
            {
                MainMenuUI.Instance.InteractionBubblesController.HideInteractionBubble(transform);
            }
        }
    }
}