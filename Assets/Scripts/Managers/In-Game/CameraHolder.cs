using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraHolder : Singleton<CameraHolder>
{
    [SerializeField] private float m_PanSpeed = 15f;
    [SerializeField] private float m_MoveThreshold = .5f;

    private float _startingPosition;

    private void Update()
    {
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
    }
    
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
