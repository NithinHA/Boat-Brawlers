namespace UI.Screens
{
    public class WeaponSelectionScreen : BaseScreen
    {
#region Button OnClicks

        public void OnClick_Back()
        {
            Hide();
        }

        public void OnClick_SelectWeapon(int type)
        {
            MainMenuController.Instance.OnWeaponChange((WeaponType) type);
        }

#endregion
    }
}

public enum WeaponType
{
    None, Hammer
}

