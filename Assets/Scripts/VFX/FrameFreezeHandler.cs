using System.Collections;
using UnityEngine;

namespace VFX
{
    public class FrameFreezeHandler : Singleton<FrameFreezeHandler>
    {
        private bool _waiting;

        public void PerformFrameFreeze(float initialDelay, float duration)
        {
            if (_waiting)
                return;
            StartCoroutine(FreezeFrameRoutine(initialDelay, duration));
        }

        private IEnumerator FreezeFrameRoutine(float initialDelay, float duration)
        {
            float originalTimeScale = Time.timeScale;
            yield return new WaitForSecondsRealtime(initialDelay);
            Time.timeScale = 0;
            _waiting = true;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = originalTimeScale;
            _waiting = false;
        }
    }
}