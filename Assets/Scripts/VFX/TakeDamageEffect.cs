using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TakeDamageEffect : Singleton<TakeDamageEffect>
{
    [SerializeField] private Volume m_PostProcessVolume;
    [SerializeField] private float m_EffectDuration = .3f;
    [Space]
    [SerializeField] private float m_VignetteStrength = .4f;
    [SerializeField] private Color m_TargetVignetteColor = Color.red;
    [Space]
    [SerializeField] private float m_ChromaticAberrationStrength = .7f;

    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private bool _isTakingDamage = false;
    private float _currentTime = 0f;

    private float _defaultVignetteIntensity = 0.2f;
    private Color _defaultVignetteColor = Color.black;

    protected override void Start()
    {
        base.Start();
        if (m_PostProcessVolume.profile.TryGet(out _vignette) &&
            m_PostProcessVolume.profile.TryGet(out _chromaticAberration))
        {
            _defaultVignetteColor = _vignette.color.value;
            _defaultVignetteIntensity = _vignette.intensity.value;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _vignette.color.value = _defaultVignetteColor;
        _vignette.intensity.value = _defaultVignetteIntensity;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            TakeDamage();
        
        if (_isTakingDamage)
        {
            _currentTime += Time.deltaTime;
            float effectStrength = Mathf.Clamp01(_currentTime / m_EffectDuration);
            
            // Animate the vignette and chromatic aberration intensity
            _vignette.intensity.value = Mathf.Lerp(_defaultVignetteIntensity, m_VignetteStrength, effectStrength);
            _vignette.color.value = Color.Lerp(_defaultVignetteColor, m_TargetVignetteColor, effectStrength);
            _chromaticAberration.intensity.value = Mathf.Lerp(0f, m_ChromaticAberrationStrength, effectStrength);

            if (_currentTime >= m_EffectDuration)
            {
                _isTakingDamage = false;
                _currentTime = 0f;
            }
        }
        else if (_vignette.intensity.value > _defaultVignetteIntensity || _vignette.color.value != _defaultVignetteColor || _chromaticAberration.intensity.value > 0f)
        {
            // Gradually fade out the effect
            _vignette.intensity.value = Mathf.Lerp(_vignette.intensity.value, _defaultVignetteIntensity, Time.deltaTime);
            _vignette.color.value = Color.Lerp(_vignette.color.value, _defaultVignetteColor, Time.deltaTime);
            _chromaticAberration.intensity.value = Mathf.Lerp(_chromaticAberration.intensity.value, 0f, Time.deltaTime);
        }
    }

    public void TakeDamage()
    {
        _isTakingDamage = true;
        _currentTime = 0f;
    }
}