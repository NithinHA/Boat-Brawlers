using BaseObjects.Player;
using Cinemachine;
using UI.Interaction;
using UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUI : Singleton<MainMenuUI>
    {
        public RectTransform MainMenuScreen;
        public FloatingJoystick FloatingJoystick;
        [Space]
        public WeaponSelectionScreen WeaponSelectionScreen;
        public RaftSelectionScreen RaftSelectionScreen;
        public MissionSelectionScreen MissionSelectionScreen;
        [Space]
        public InteractionBubblesController InteractionBubblesController;

        protected override void Start()
        {
            base.Start();
            GameManager.Instance.PlayerMovementToggle?.Invoke(false);
            MenuCameraSwitcher.SwitchCamera(MainMenuController.Instance.MenuCam);
            GameManager.Instance.PlayerMovementToggle += OnPlayerMovementToggle;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (GameManager.Instance != null)
                GameManager.Instance.PlayerMovementToggle -= OnPlayerMovementToggle;
        }

        /// <summary>
        /// temporary.. irrespective of mission index, loads level based on Boat selected.
        /// </summary>
        public void LoadMission(LevelNames type)
        {
            SceneManager.LoadScene(type.ToString());
        }

#region Button OnClicks

        public void OnClick_Play()
        {
            MenuCameraSwitcher.SwitchCamera(MainMenuController.Instance.PlayerCam);
            MainMenuScreen.gameObject.SetActive(false);
            GameManager.Instance.PlayerMovementToggle?.Invoke(true);
        }

        public void OnClick_Quit()
        {
            Application.Quit();
        }

        public void OnClick_MissionScreen()
        {
            MissionSelectionScreen.Show();
        }

#endregion


        private void OnPlayerMovementToggle(bool active)
        {
            FloatingJoystick.gameObject.SetActive(active);
        }


    }
}