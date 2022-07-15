using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStaticRotatorMainMenu : MonoBehaviour
{
    [SerializeField] private float m_Speed = 50f;
    void Update()
    {
        transform.RotateAround(transform.position, transform.up, Time.deltaTime * m_Speed);
    }
}
