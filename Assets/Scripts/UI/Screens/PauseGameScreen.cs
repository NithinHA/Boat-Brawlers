using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class PauseGameScreen : BaseScreen
    {
        [SerializeField] private RectTransform m_DiskPanel;
        [SerializeField] private GameObject m_SoundOn;
        [SerializeField] private GameObject m_SoundOff;

        private bool _isSoundEnabled;

        public override void Show()
        {
            base.Show();
            // _isSoundEnabled = AudioManager.Instance.IsMuted
            ToggleSoundButton();
            Time.timeScale = 0;
            m_DiskPanel.DORotate(new Vector3(0, 0, -180), .3f).SetEase(Ease.OutBack)
                .SetUpdate(true);
        }

        public override void Hide()
        {
            base.Hide();
            Time.timeScale = 1;
        }

        private void ToggleSoundButton()
        {
            if (_isSoundEnabled)
            {
                m_SoundOn.SetActive(true);
                m_SoundOff.SetActive(false);
            }
            else
            {
                m_SoundOff.SetActive(true);
                m_SoundOn.SetActive(false);
            }
        }

#region Button OnClicks

        public void OnClick_Resume()
        {
            m_DiskPanel.DORotate(new Vector3(0, 0, -90), .3f).SetEase(Ease.InBack)
                .OnComplete(() => InGameUI.Instance.OnGameResume())
                .SetUpdate(true);
        }

        public void OnClick_Home()
        {
            Time.timeScale = 1;
            LevelManager.Instance.GoToMainMenu();
        }

        public void OnClick_Retry()
        {
            Time.timeScale = 1;
            LevelManager.Instance.RetryLevel();
        }

        public void OnClick_ToggleSound()
        {
            _isSoundEnabled = !_isSoundEnabled;
            ToggleSoundButton();
        }

#endregion

    }
}