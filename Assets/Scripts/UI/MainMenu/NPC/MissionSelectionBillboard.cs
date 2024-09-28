using UI;

namespace NPC
{
    public class MissionSelectionBillboard : BaseNpcBehaviour
    {
        protected override void OnInteract()
        {
            base.OnInteract();
            MainMenuUI.Instance.MissionSelectionScreen.Show(OnReset);
        }

        protected override void OnReset()
        {
            base.OnReset();
        }
    }
}