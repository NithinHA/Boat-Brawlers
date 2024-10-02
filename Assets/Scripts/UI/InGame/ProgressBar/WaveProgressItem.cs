using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ProgressBar
{
    public class WaveProgressItem : MonoBehaviour
    {
        [SerializeField] private Image m_FillImage;
        public float Target;
        [Space]
        [SerializeField] private RectTransform m_WaveIndicatorBg;
        [SerializeField] private Image m_WaveIndicator;
        [SerializeField] private Color m_WaveIncompleteColor = Color.yellow;
        [SerializeField] private Color m_WaveCompleteColor = Color.red;

        private Vector2 _indicatorScaleRange = new Vector2(1f, 1.4f);
        private Tween _indicatorScalingTween = null;

        public void Init(float target)
        {
            this.Target = target;
            SetProgress(0);
            ToggleWaveCompletionColor(false);
        }
        
        public void SetProgress(float amount)
        {
            m_FillImage.fillAmount = amount / Target;
        }

        /// <summary>
        /// Toggle looping anim for waveIndicator
        /// </summary>
        public void ToggleWaveIndicator(bool active)
        {
            if (active)
            {
                _indicatorScalingTween = m_WaveIndicatorBg.DOScale(Vector3.one * _indicatorScaleRange.y, .5f)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                if (_indicatorScalingTween != null)
                    _indicatorScalingTween.Kill();

                m_WaveIndicatorBg.localScale = Vector3.one * _indicatorScaleRange.x;
            }
        }

        public void ToggleWaveCompletionColor(bool isComplete)
        {
            m_WaveIndicator.color = isComplete ? m_WaveCompleteColor : m_WaveIncompleteColor;
        }
    }
}
