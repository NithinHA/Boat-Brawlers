using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ProgressBar
{
    [RequireComponent(typeof(LayoutGroup))]
    public class WavesSystemProgressBar : MonoBehaviour
    {
        [SerializeField] private WaveProgressItem m_ProgressItemPrefab;

        private WaveInfo[] _levelWaveInfo;
        private WaveProgressItem[] _progressItems;

        public void CreateProgressBar()
        {
            _levelWaveInfo = EnemyWaveSystem.Instance.Waves;
            _progressItems = new WaveProgressItem[_levelWaveInfo.Length];

            for (int i = 0; i < _levelWaveInfo.Length; i++)
            {
                WaveInfo wave = _levelWaveInfo[i];
                WaveProgressItem item = Instantiate(m_ProgressItemPrefab, transform);
                item.Init(wave.TimeBeforeWaveBegin);
                _progressItems[i] = item;
            }

            StartCoroutine(DisableLayoutGroupPostCreationRoutine());
        }

        private IEnumerator DisableLayoutGroupPostCreationRoutine()
        {
            yield return new WaitForEndOfFrame();
            GetComponent<LayoutGroup>().enabled = false;
        }

        /// <summary>
        /// Sets the current wave progress bar fill amount
        /// </summary>
        /// <param name="amount"> Range (0, currentWave.TimeBeforeWaveBegin) </param>
        public void SetProgress(float amount)
        {
            _progressItems[EnemyWaveSystem.Instance.CurrentWaveIndex].SetProgress(amount);
        }

        public void ToggleWaveBlinkIndicator(bool active)
        {
            int currentIndex = EnemyWaveSystem.Instance.CurrentWaveIndex;
            _progressItems[currentIndex].ToggleWaveIndicator(active);
            _progressItems[currentIndex].ToggleWaveCompletionColor(true);
        }
    }
}
