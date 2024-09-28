using UI;

namespace NPC
{
    public class WeaponSelectionNpcBehaviour : BaseNpcBehaviour
    {
        private void Start()
        {
            m_TargetCamera = MainMenuController.Instance.WeaponSelectionCam;
        }

        protected override void OnInteract()
        {
            base.OnInteract();
            MainMenuUI.Instance.WeaponSelectionScreen.Show(OnReset);
        }

        protected override void OnReset()
        {
            base.OnReset();
        }
    }
}