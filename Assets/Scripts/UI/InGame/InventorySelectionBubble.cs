using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace  UI
{
    [RequireComponent(typeof(RectTransform))]
    public class InventorySelectionBubble : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_Label;
        private Transform _target;
        private Vector3 _bubbleOffset;
        private RectTransform _rect;
        private Camera _mainCam;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
            _mainCam = Camera.main;
        }

        private void Update()
        {
            if (_target != null)
            {
                _rect.position = _mainCam.WorldToScreenPoint(_target.position) + _bubbleOffset;
            }
        }

        public void SetTarget(Transform target, Vector3 offset)
        {
            this._target = target;
            this._bubbleOffset = offset;
            m_Label.text = target.name;
            gameObject.SetActive(true);
        }

        public void RemoveTarget()
        {
            this._target = null;
            gameObject.SetActive(false);
        }
    }
}
