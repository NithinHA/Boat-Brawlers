using System;
using UnityEngine;

namespace UI.Screens
{
    public class BaseScreen : MonoBehaviour
    {
        public GameObject Container;
        private Action _onClose = null;

        private void SetContainer()
        {
            if (Container == null)
                Container = gameObject;
        }
        
        public virtual void Show()
        {
            SetContainer();
            Container.SetActive(true);
        }

        public virtual void Show(Action onClose = null)
        {
            _onClose = onClose;
            Show();
        }

        public virtual void Hide()
        {
            if (_onClose != null)
            {
                _onClose.Invoke();
                _onClose = null;
            }

            Container.SetActive(false);
        }
    }
}