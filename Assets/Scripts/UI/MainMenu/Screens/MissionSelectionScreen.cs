using System.Collections;
using System.Collections.Generic;
using BaseObjects.Player;
using UI.Screens;
using UnityEngine;

namespace UI.Screens
{
    public class MissionSelectionScreen : BaseScreen
    {
        public override void Show()
        {
            base.Show();
            Player.Instance.PlayerMovement.IsMovementEnabled = false;
        }

        public override void Hide()
        {
            base.Hide();
            Player.Instance.PlayerMovement.IsMovementEnabled = true;
        }

#region Button OnClicks

        public void OnClick_Back()
        {
            Hide();
        }
        
        public void OnClick_SelectMission(int index)
        {
            MainMenuUI.Instance.LoadMission(index);
        }
        
#endregion
    }
}
