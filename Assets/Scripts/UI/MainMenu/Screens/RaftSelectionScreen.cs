namespace UI.Screens
{
    public class RaftSelectionScreen : BaseScreen
    {
#region Button OnClicks

        public void OnClick_Back()
        {
            Hide();
        }

        public void OnClick_SelectRaft(int type)
        {
            MainMenuController.Instance.OnRaftChange((RaftType) type);
        }

#endregion
    }
}

public enum RaftType
{
    Simple, Seafaring
}