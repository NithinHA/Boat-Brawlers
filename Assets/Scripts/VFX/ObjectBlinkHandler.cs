using DG.Tweening;
using UnityEngine;

namespace VFX
{
    public class ObjectBlinkHandler : Singleton<ObjectBlinkHandler>
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        /// <summary>
        /// Handles blinking of an object's emission by creating a material instance.
        /// </summary>
        /// <param name="render">The renderer of the object to blink.</param>
        /// <param name="color">The emission color set for blink.</param>
        /// <param name="duration">The total time the object should blink.</param>
        /// <param name="intensity">The maximum emission intensity.</param>
        public void BlinkOnce(Renderer render, Color color, float duration = .5f, float intensity = 1)
        {
            if (render == null) return;

            // Clone the material so it affects only this specific object
            Material clonedMaterial = render.material;
            clonedMaterial.EnableKeyword("_EMISSION");

            DOTween.To(() => 0f, emission => { clonedMaterial.SetColor(EmissionColor, color * emission); }, intensity, duration)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => StopBlinking(clonedMaterial));
        }

        /// <summary>
        /// Stops blinking and resets the emission after a specified duration.
        /// </summary>
        private void StopBlinking(Material material)
        {
            if (material != null)
            {
                material.SetColor(EmissionColor, Color.black);
                material.DisableKeyword("_EMISSION");
            }
        }
    }
}