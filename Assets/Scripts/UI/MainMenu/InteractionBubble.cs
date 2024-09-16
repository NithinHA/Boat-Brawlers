using System;
using UnityEngine;

namespace UI.Interaction
{
    public class InteractionBubble : MonoBehaviour
    {
        [SerializeField] private Transform m_RectTransform;

        private Action _onInteract;
        private bool _active;

        private Transform _target;
        private Vector3 _offset;
        private Camera _mainCam;

        private void Update()
        {
            if (!_active)
                return;

            UpdatePosition();
        }

        public void SetupBubble(Transform target, Vector3 offset, Camera mainCam, Action onInteract = null)
        {
            _active = true;
            gameObject.SetActive(true);

            this._target = target;
            this._offset = offset;
            this._mainCam = mainCam;
            this._onInteract = onInteract;
            if (m_RectTransform == null)
                m_RectTransform = GetComponent<RectTransform>();
        }

        public void ResetBubble()
        {
            _active = false;
            gameObject.SetActive(false);
        }

        private void UpdatePosition()
        {
            Vector3 screenPos = _mainCam.WorldToScreenPoint(_target.position + _offset);
            m_RectTransform.position = screenPos;
        }

        public void OnButtonClick()
        {
            _onInteract?.Invoke();
        }
    }
}