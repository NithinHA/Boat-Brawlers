using UnityEngine;
using BaseObjects.Player;

namespace UI
{
    public class PlayerControlPanelUI : MonoBehaviour
    {
        [SerializeField] private FloatingJoystick m_FloatingJoystick;
        [SerializeField] private AttackButtonUI m_AttackButton;
        [Header("Pick & Drop")]
        [SerializeField] private GameObject m_Pick_VariantMobile;
        [SerializeField] private GameObject m_Pick_VariantPC;
        [Space]
        [SerializeField] private GameObject m_Drop_VariantMobile;
        [SerializeField] private GameObject m_Drop_VariantPC;

        private void Awake()
        {
            Reset();
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            SetupForMobile();
#else
            SetupForPC();
#endif
        }

        private void Reset()
        {
            m_FloatingJoystick.gameObject.SetActive(false);
            m_Pick_VariantMobile.SetActive(false);
            m_Pick_VariantPC.SetActive(false);
            m_Drop_VariantMobile.SetActive(false);
            m_Drop_VariantPC.SetActive(false);
        }

        private void SetupForMobile()
        {
            m_FloatingJoystick.gameObject.SetActive(true);
            m_Pick_VariantMobile.SetActive(true);
            m_Drop_VariantMobile.SetActive(true);
            m_AttackButton.SetupVisualPerPlatform(true); ;
        }

        private void SetupForPC()
        {
            m_FloatingJoystick.gameObject.SetActive(false);
            m_Pick_VariantPC.SetActive(true);
            m_Drop_VariantPC.SetActive(true);
            m_AttackButton.SetupVisualPerPlatform(false);
        }

#region Button OnClicks

        public void OnClick_Pick()
        {
            Player.Instance.PlayerInteraction.PickItem();
        }

        public void OnClick_Drop()
        {
            Player.Instance.PlayerInteraction.DropItem();
        }

#endregion
    }
}