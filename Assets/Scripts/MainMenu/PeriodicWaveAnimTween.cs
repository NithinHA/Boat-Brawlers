using UnityEngine;
using DG.Tweening;

public class PeriodicWaveAnimTween : MonoBehaviour
{
    [SerializeField] private float m_Amplitude = .2f;
    [SerializeField] private float m_Duration = 1f;
    [SerializeField] private float m_HorizontalNoiseAmount = 0.2f;  // Amount of random horizontal movement
    [SerializeField] private float m_NoiseDuration = 2f;          // How fast the horizontal noise changes

    private Vector3 _startPos;     // Starting position of the object

    void Start()
    {
        _startPos = transform.position;
        StartWaveAnimation();
        StartHorizontalNoise();
    }

    void StartWaveAnimation()
    {
        transform.DOMoveY(_startPos.y + m_Amplitude, m_Duration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    void StartHorizontalNoise()
    {
        InvokeRepeating(nameof(ApplyRandomHorizontalNoise), 0, m_NoiseDuration);
    }

    void ApplyRandomHorizontalNoise()
    {
        float randomX = Random.Range(-m_HorizontalNoiseAmount, m_HorizontalNoiseAmount);
        transform.DOMoveX(_startPos.x + randomX, m_NoiseDuration)
            .SetEase(Ease.InOutSine);
    }
}