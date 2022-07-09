using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;

namespace BaseObjects.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Player m_Player;
        [Space]
        [SerializeField] private float m_DefaultCooldownTime = .5f;

        private float _timer = 0f;
        private bool _canAttack = true;
        private Weapon _heldWeapon;
        public bool HasWeapon => _heldWeapon != null;
        private float _cachedRigWeight;

        private const int SimpleAttackTypes = 2;

        private void Start()
        {
            m_Player.PlayerInteraction.OnItemPicked += OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped += OnItemDropped;
        }

        private void OnDestroy()
        {
            m_Player.PlayerInteraction.OnItemPicked -= OnItemPicked;
            m_Player.PlayerInteraction.OnItemDropped -= OnItemDropped;
        }

        private void Update()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                return;
            }
            
            if (Input.GetKeyDown(KeyCode.K))
            {
                Attack();
                _timer = !HasWeapon ? m_DefaultCooldownTime : _heldWeapon.AttackCooldown;
            }
        }

        public void Attack()
        {
            _cachedRigWeight = m_Player.Rig.weight;
            m_Player.Rig.weight = 0;

            if (HasWeapon)
            {
                WeaponAttack();
            }
            else
            {
                SimpleAttack();
            }
        }

        private void SimpleAttack()
        {
            AnimateAttack(0, Random.Range(0, SimpleAttackTypes));
        }

        private void WeaponAttack()
        {
            _heldWeapon.Attack();
            AnimateAttack((int) _heldWeapon.Type, Random.Range(0, _heldWeapon.AttackAnimVarities));
        }

        private void AnimateAttack(int weaponIndex, int attackIndex)
        {
            m_Player.Anim.SetInteger(Constants.Animation.WEAPON_INDEX, weaponIndex);
            m_Player.Anim.SetInteger(Constants.Animation.ATTACK_INDEX, attackIndex);
            m_Player.Anim.SetTrigger(Constants.Animation.ATTACK);
        }


#region Event lisetners
        
        private void OnItemPicked(InteractableObject obj)
        {
            _heldWeapon = obj as Weapon;
        }

        private void OnItemDropped(InteractableObject obj)
        {
            _heldWeapon = null;
        }

        public void OnAttackAnimDisableMovement()
        {
            m_Player.PlayerMovement.IsMovementEnabled = false;
        }
        
        public void OnAttackAnimComplete()
        {
            DOTween.To(() => m_Player.Rig.weight, x => m_Player.Rig.weight = x, _cachedRigWeight, .01f);
            m_Player.PlayerMovement.IsMovementEnabled = true;
        }

#endregion
    }
}
