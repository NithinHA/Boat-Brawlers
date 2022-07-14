using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UI.ProgressBar;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Screens
{
    public class InGameScreen : BaseScreen
    {
        [SerializeField] private WavesSystemProgressBar m_ProgressBar;
        [Space]
        [SerializeField] private RectTransform m_WaveInfoRect;
        [SerializeField] private TextMeshProUGUI m_WaveInfoText;
        [SerializeField] private string[] m_WaveMessages;

        private void Start()
        {
            m_ProgressBar.CreateProgressBar();
        }

        public void UpdateProgressBar(float timeSinceLastWave)
        {
            m_ProgressBar.SetProgress(timeSinceLastWave);
        }

        public void ToggleBlinkWaveIndicator(bool active)
        {
            m_ProgressBar.ToggleWaveBlinkIndicator(active);
        }
        
        public void DisplayWaveInfo(int waveIndex)
        {
            string waveInfoStr = m_WaveMessages[Random.Range(0, m_WaveMessages.Length)];
            m_WaveInfoText.text = string.Format(waveInfoStr, waveIndex + 1);
            m_WaveInfoRect.localPosition = Vector3.down * Screen.height;
            m_WaveInfoRect.gameObject.SetActive(true);
            
            Sequence waveInfoSequence = DOTween.Sequence();
            waveInfoSequence.Append(m_WaveInfoRect.DOLocalMoveY(0, .3f).SetEase(Ease.OutBack));
            waveInfoSequence.AppendInterval(1);
            waveInfoSequence.Append(m_WaveInfoRect.DOLocalMoveY(Screen.height, .3f).SetEase(Ease.InBack));
            waveInfoSequence.AppendCallback(() =>
            {
                m_WaveInfoRect.gameObject.SetActive(false);
                m_WaveInfoRect.localPosition = Vector3.down * Screen.height;
            });
        }


#region Button OnClicks

        public void OnClick_Pause()
        {
            InGameUI.Instance.OnGamePause();
        }

#endregion
    }
}