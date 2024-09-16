namespace UI.Screens
{
    public class MissionSelectionScreen : BaseScreen
    {
#region Button OnClicks

        public void OnClick_Back()
        {
            Hide();
        }
        
        public void OnClick_SelectMission(int type)
        {
            MainMenuUI.Instance.LoadMission((LevelNames) type);
        }
        
#endregion
    }
}

public enum LevelNames
{
    Level_1 = 1, Level_2 = 2
}
