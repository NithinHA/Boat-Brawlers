using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    public class InGameScreen : BaseScreen
    {
        [SerializeField] private Slider m_ProgressBar;
        [SerializeField] private Image[] m_WaveIndicator;
        [Space]
        [SerializeField] private RectTransform m_WaveInfoRect;
        [SerializeField] private TextMeshProUGUI m_WaveInfoText;
        [SerializeField] private string[] m_WaveMessages;

        private readonly Vector3 _centerPos = Vector3.zero;
        private readonly Vector3 _downPos = new Vector3(-Screen.height, 0, 0);
        private readonly Vector3 _topPos = new Vector3(Screen.height, 0, 0);

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Y))
                DisplayWaveInfo(0);
        }

        public void UpdateProgressBar()
        {
            
        }

        public void ToggleBlinkWaveIndicator(int waveIndex, bool active)
        {
            
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