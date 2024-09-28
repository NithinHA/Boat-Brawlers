using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraHolder : Singleton<CameraHolder>
{
    [SerializeField] private float m_PanSpeed = 15f;
    [SerializeField] private float m_MoveThreshold = .5f;
    [Header("Virtual cams")]
    public CinemachineVirtualCamera PlayerCam;
    public CinemachineVirtualCamera LevelWin;
    [Header("Camera shake")]
    [SerializeField] private float shakeDuration = 0.5f;
    
    private CinemachineBasicMultiChannelPerlin _noise;
    private float _shakeTimer;
    private float _startingPosition;

    protected override void Start()
    {
        base.Start();
        MenuCameraSwitcher.SwitchCamera(PlayerCam);
        _noise = PlayerCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void OnEnable()
    {
        MenuCameraSwitcher.Register(PlayerCam);
        MenuCameraSwitcher.Register(LevelWin);
    }

    private void OnDisable()
    {
        MenuCameraSwitcher.Unregister(PlayerCam);
        MenuCameraSwitcher.Unregister(LevelWin);
    }
    
    private void Update()
    {
        HandleCamRotationX();
        HandleCameraShake();
    }
    
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void HandleCamRotationX()
    {
#if !UNITY_EDITOR
        if (Input.touchCount == 1 && !IsPointerOverUIObject())
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    _startingPosition = touch.position.x;
                    break;
                case TouchPhase.Moved:
                    if (touch.position.x - _startingPosition > m_MoveThreshold)
                    {
                        transform.Rotate(0f, touch.deltaPosition.x * m_PanSpeed * Time.deltaTime, 0f);
                    }
                    else if (touch.position.x - _startingPosition < -m_MoveThreshold)
                    {
                        transform.Rotate(0f, touch.deltaPosition.x * m_PanSpeed * Time.deltaTime, 0f);
                    }
                    break;
            }
        }
#else
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(0f, -3 * m_PanSpeed * Time.deltaTime, 0f);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(0f, 3 * m_PanSpeed * Time.deltaTime, 0f);
        }
#endif
    }

    private void HandleCameraShake()
    {
        if (_shakeTimer > 0)
        {
            _shakeTimer -= Time.deltaTime;
            if (_shakeTimer <= 0f)
            {
                // Reset the shake when the timer runs out
                _noise.m_AmplitudeGain = 0f;
            }
        }
    }

    public void TriggerCameraShake(float intensity = 2, float frequency = 1)
    {
        _noise.m_AmplitudeGain = intensity;
        _noise.m_FrequencyGain = frequency;
        _shakeTimer = shakeDuration;
    }

}
