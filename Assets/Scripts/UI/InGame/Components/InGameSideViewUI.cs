using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InGameSideViewUI : MonoBehaviour
    {
        [SerializeField] private Image m_Meter;
        [SerializeField] private List<Color> m_ColorDistribution = new List<Color>() {Color.green, Color.yellow, Color.red};

        private void Update()
        {
            if (RaftController_Custom.Instance != null)
            {
                UpdateMeter(RaftController_Custom.Instance.GetTiltPercent01());
            }
        }

        private void UpdateMeter(float amount)
        {
            m_Meter.fillAmount = amount;
            m_Meter.color = GetColorFromDistribution(amount);
        }

        private Color GetColorFromDistribution(float amount)
        {
            // Calculate the exact position in the gradient list
            float position = amount * (m_ColorDistribution.Count - 1);
            // Get the indices of the two colors to interpolate between
            int index1 = Mathf.FloorToInt(position);
            int index2 = Mathf.CeilToInt(position);
            // If position is exactly at an index, return that color
            if (index1 == index2) return m_ColorDistribution[index1];
            // Interpolate between the two colors
            float t = position - index1;  // Interpolation factor
            return Color.Lerp(m_ColorDistribution[index1], m_ColorDistribution[index2], t);
            
        }
    }
}