using System;
using BaseObjects.Player;
using Cinemachine;
using UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUI : Singleton<MainMenuUI>
    {
        [SerializeField] private CinemachineVirtualCamera m_MenuCam;
        [SerializeField] private CinemachineVirtualCamera m_PlayerCam;
        [Space]
        public RectTransform MainMenuScreen;
        public RaftSelectionScreen RaftSelectionScreen;
        public MissionSelectionScreen MissionSelectionScreen;

        protected override void Start()
        {
            MenuCameraSwitcher.SwitchCamera(m_MenuCam);
        }

        private void OnEnable()
        {
            MenuCameraSwitcher.Register(m_MenuCam);
            MenuCameraSwitcher.Register(m_PlayerCam);
        }

        private void OnDisable()
        {
            MenuCameraSwitcher.Unregister(m_MenuCam);
            MenuCameraSwitcher.Unregister(m_PlayerCam);
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
            if (MenuCameraSwitcher.ActiveCamera == m_MenuCam)
            {
                MenuCameraSwitcher.SwitchCamera(m_PlayerCam);
                MainMenuScreen.gameObject.SetActive(false);
            }

            Player.Instance.PlayerMovement.IsMovementEnabled = true;
        }

        public void OnClick_Quit()
        {
            Application.Quit();
        }

        public void OnClick_RaftScreen()
        {
            RaftSelectionScreen.Show();
        }

        public void OnClick_MissionScreen()
        {
            MissionSelectionScreen.Show();
        }

#endregion

    }
}