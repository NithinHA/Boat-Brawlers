using System;
using System.Collections;
using BaseObjects;
using BaseObjects.Player;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AttackButtonUI : MonoBehaviour
{
    [SerializeField] private Image m_FillImage;
    [SerializeField] private RectTransform m_AtkButtonRect;
    [SerializeField] private float m_TimeToFill = 2f;
    [Space]
    [SerializeField] private GameObject m_SpaceText;

    private PlayerAttack _playerAttack;
    private float _fillDelta;
    private bool _isCharged = false;
    private Coroutine _chargeRoutine;
    
    private void Start()
    {
        _playerAttack = Player.Instance.PlayerAttack;
        _fillDelta = 1 / m_TimeToFill;

        Player.Instance.PlayerInteraction.OnItemPicked += ResetButtonCharge;
        Player.Instance.PlayerInteraction.OnItemDropped += ResetButtonCharge;
    }

    private void OnDestroy()
    {
        Player.Instance.PlayerInteraction.OnItemPicked -= ResetButtonCharge;
        Player.Instance.PlayerInteraction.OnItemDropped -= ResetButtonCharge;
    }

    private IEnumerator WaitAndChargeRoutine()
    {
        yield return new WaitForSeconds(.2f);

        while (m_FillImage.fillAmount < 1)
        {
            m_FillImage.fillAmount += _fillDelta * Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        _isCharged = true;
        m_AtkButtonRect.DOScale(Vector3.one * 1.2f, .1f)
            .OnComplete(() => m_AtkButtonRect.DOScale(Vector3.one, .1f));
    }

    private void ResetButtonCharge(InteractableObject obj = null)
    {
        _isCharged = false;
        if (_chargeRoutine != null)
        {
            StopCoroutine(_chargeRoutine);
            _chargeRoutine = null;
        }

        float fillValue = m_FillImage.fillAmount;
        _isCharged = false;
        DOTween.To(() => fillValue, (x) => fillValue = x, 0, .5f)
            .OnUpdate(() => m_FillImage.fillAmount = fillValue)
            .SetEase(Ease.InOutCubic);
    }

#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IOS)
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPointerDown_Attack();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            OnPointerUp_Attack();
        }
    }
#endif

    public void OnPointerDown_Attack()
    {
        // if held weapon
        if (_playerAttack.HasWeapon)
        {
            _chargeRoutine = StartCoroutine(WaitAndChargeRoutine());
        }
        // wait for .2 sec and start filling
        // else
        // do nothing
    }

    public void OnPointerUp_Attack()
    {
        // if filled Heavy Attack
        if (_isCharged)
        {
            _playerAttack.Attack(true);
        }
        // else Simple Attack
        else
        {
            _playerAttack.Attack();
        }

        ResetButtonCharge();
    }

#region Handle platform specific items

    public void SetupVisualPerPlatform(bool isMobile)
    {
        m_SpaceText.SetActive(!isMobile);
    }

#endregion
}
