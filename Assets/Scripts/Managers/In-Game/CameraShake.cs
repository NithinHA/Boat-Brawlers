using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Vector3 Amount = new Vector3(1f, 1f, 0);
    public float Duration = 1;
    public float Speed = 10;
    public AnimationCurve Curve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    /// <summary>
    /// Set it to true: The camera position is set in reference to the old position of the camera
    /// Set it to false: The camera position is set in absolute values or is fixed to an object
    /// </summary>
    public bool DeltaMovement = true;

    private Camera _mainCam;
    private float _time = 0;
    private Vector3 _lastPos;
    private Vector3 _nextPos;
    private float _lastFov;
    private float _nextFov;
    private bool _destroyAfterPlay;

    private void Awake()
    {
        _mainCam = Camera.main;
    }

    public static void ShakeOnce(float duration = 1f, float speed = 10f, Vector3? amount = null, Camera camera = null, bool deltaMovement = true, AnimationCurve curve = null)
    {
        Debug.Log("Cam shacked at: " + speed);
        //set data
        var instance = ((camera != null) ? camera : Camera.main).gameObject.AddComponent<CameraShake>();
        instance.Duration = duration;
        instance.Speed = speed;
        if (amount != null)
            instance.Amount = (Vector3)amount;
        if (curve != null)
            instance.Curve = curve;
        instance.DeltaMovement = deltaMovement;

        //one time
        instance._destroyAfterPlay = true;
        instance.Shake();
    }

    public void Shake()
    {
        ResetCam();
        _time = Duration;
    }

    private void LateUpdate()
    {
        if (_time > 0)
        {
            //do something
            _time -= Time.deltaTime;
            if (_time > 0)
            {
                //next position based on perlin noise
                _nextPos = (Mathf.PerlinNoise(_time * Speed, _time * Speed * 2) - 0.5f) * Amount.x * transform.right * Curve.Evaluate(1f - _time / Duration) +
                          (Mathf.PerlinNoise(_time * Speed * 2, _time * Speed) - 0.5f) * Amount.y * transform.up * Curve.Evaluate(1f - _time / Duration);
                _nextFov = (Mathf.PerlinNoise(_time * Speed * 2, _time * Speed * 2) - 0.5f) * Amount.z * Curve.Evaluate(1f - _time / Duration);

                _mainCam.fieldOfView += (_nextFov - _lastFov);
                _mainCam.transform.Translate(DeltaMovement ? (_nextPos - _lastPos) : _nextPos);

                _lastPos = _nextPos;
                _lastFov = _nextFov;
            }
            else
            {
                //last frame
                ResetCam();
                if (_destroyAfterPlay)
                    Destroy(this);
            }
        }
    }

    private void ResetCam()
    {
        //reset the last delta
        _mainCam.transform.Translate(DeltaMovement ? -_lastPos : Vector3.zero);
        _mainCam.fieldOfView -= _lastFov;

        //clear values
        _lastPos = _nextPos = Vector3.zero;
        _lastFov = _nextFov = 0f;
    }
}
