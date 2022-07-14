using System;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        /*
        public MainMenuUI MainMenuUI;
        public InGameUI InGameUI;
        public PauseGameUI PauseGameUI;
        public EndGameUI EndGameUI;

        public Action<string> OnMainMenuUI_ButtonClick;
        public Action<string> OnPauseMenuUI_ButtonClick;
        public Action<string> OnGameUI_ButtonClick;
        public Action<string> OnEndGameUI_ButtonClick;

        protected override void Awake()
        {
            base.Awake();

            OnMainMenuUI_ButtonClick += OnMainMenuUIButtonClick;
            OnGameUI_ButtonClick += OnGameUIButtonClick;
            OnPauseMenuUI_ButtonClick += OnPauseMenuUIButtonClick;

            SetupUI();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            OnMainMenuUI_ButtonClick -= OnMainMenuUIButtonClick;
            OnGameUI_ButtonClick -= OnGameUIButtonClick;
            OnPauseMenuUI_ButtonClick -= OnPauseMenuUIButtonClick;
        }

        private void SetupUI()
        {
            MainMenuUI.gameObject.SetActive(true);
            InGameUI.gameObject.SetActive(false);
            PauseGameUI.gameObject.SetActive(false);
            EndGameUI.gameObject.SetActive(false);
        }

        public void OnGameStart()
        {
            MainMenuUI.gameObject.SetActive(false);
            InGameUI.gameObject.SetActive(true);
        }
        
        public void OnGameEnd(bool isWin)
        {
            InGameUI.gameObject.SetActive(false);
            EndGameUI.gameObject.SetActive(true);
        }

#region UIEvent listeners

        private void OnMainMenuUIButtonClick(string btnName)
        {
            switch (btnName)
            {
                case Constants.UIEvents.OnClick_Play:
                    SceneManager.LoadScene(1);
                    break;
            }
        }

        private void OnGameUIButtonClick(string btnName)
        {
            switch (btnName)
            {
                case Constants.UIEvents.OnClick_Pause:
                    PauseGameUI.gameObject.SetActive(true);
                    break;
            }
        }

        private void OnPauseMenuUIButtonClick(string btnName)
        {
            switch (btnName)
            {
                case Constants.UIEvents.OnClick_Resume:
                    PauseGameUI.gameObject.SetActive(false);
                    break;
                case Constants.UIEvents.OnClick_Home:
                    PauseGameUI.gameObject.SetActive(false);
                    InGameUI.gameObject.SetActive(false);
                    MainMenuUI.gameObject.SetActive(true);
                    break;
            }
        }

        private void OnEndGameUIButtonClick(string btnName)
        {
            
        }

#endregion
        */
    }
}