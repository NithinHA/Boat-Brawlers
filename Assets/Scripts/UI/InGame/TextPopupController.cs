using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

namespace UI
{
    public class TextPopupController : MonoBehaviour
    {
        [SerializeField] private Text m_PopupTextPrefab;
        [SerializeField] private float m_Delay = .6f;

        public void Show(string text, Vector3 worldPosition)
        {
            Text popup = Instantiate(m_PopupTextPrefab, transform);
            popup.text = text;
            Transform popupTransform = popup.transform;

            // set position
            popupTransform.position = Camera.main.WorldToScreenPoint(worldPosition);

            // animate in
            popupTransform.localScale = Vector3.zero;
            popupTransform.DOScale(Vector3.one, .2f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                DOVirtual.DelayedCall(m_Delay, () => {
                    popupTransform.DOScale(Vector3.zero, .1f).SetEase(Ease.InBack).OnComplete(() => {
                        Destroy(popupTransform.gameObject);
                    });
                });
            });
        }
    }
}