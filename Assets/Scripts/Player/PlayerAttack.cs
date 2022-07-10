using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;

namespace BaseObjects.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        [Header("Attack info")]
        [SerializeField] private float m_DefaultAttackCooldownTime = .5f;
        [SerializeField] private float[] m_AnimWeights;
        [SerializeField] private float m_SimpleAttackDamage = 20f;

        private float _attackCooldownTimer = 0f;
        private bool _canAttack = true;
        private Weapon _heldWeapon;
        public bool HasWeapon => _heldWeapon != null;
        public float CurrentDamageAmount;

        [Header("Hit info")]
        [SerializeField] private float m_HitKnockbackForce = 400f;
        [SerializeField] private float m_DefaultDamageCooldownTime = 4f;
        [SerializeField] private GameObject[] m_DefaultHitboxes;

        private float _damageCooldownTimer = 0f;
        private List<GameObject> _playerHitboxes = new List<GameObject>();

        private float _cachedRigWeight;


#region Unity callbacks

        private void Start()
        {
            CurrentDamageAmount = m_SimpleAttackDamage;            // player starts barehanded

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
            if (_attackCooldownTimer > 0)
            {
                _attackCooldownTimer -= Time.deltaTime;
            }

            if (_damageCooldownTimer > 0)
            {
                _damageCooldownTimer -= Time.deltaTime;
            }
            
            if (Input.GetKeyDown(KeyCode.K))
                Attack();
            else if (Input.GetKeyDown(KeyCode.O))
                TakeDamage(transform.position + transform.forward);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Constants.Tags.ENEMY_ATTACK_HITBOX))
            {
                TakeDamage(other.ClosestPoint(transform.position));
                // enemy.HitSuccess();
            }
        }

#endregion

        public void Attack()
        {
            if (_attackCooldownTimer > 0)
            {
                Debug.Log("=> wait for attack cooldown");
                return;
            }

            _attackCooldownTimer = !HasWeapon ? m_DefaultAttackCooldownTime : _heldWeapon.AttackCooldown;
            _cachedRigWeight = m_Player.Rig.weight;
            m_Player.Rig.weight = 0;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;

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
            int index = Utilities.GetWeightedRandomIndex(m_AnimWeights);
            AnimateAttack(0, index);
        }

        private void WeaponAttack()
        {
            _heldWeapon.Attack();
            int index = _heldWeapon.GetClipIndex();
            AnimateAttack((int) _heldWeapon.Type, index);
        }

        private void AnimateAttack(int weaponIndex, int attackIndex)
        {
            m_Player.Anim.SetInteger(Constants.Animation.WEAPON_INDEX, weaponIndex);
            m_Player.Anim.SetInteger(Constants.Animation.ATTACK_INDEX, attackIndex);
            m_Player.Anim.SetTrigger(Constants.Animation.ATTACK);
        }

        public void TakeDamage(Vector3 hitPoint)
        {
            if (_damageCooldownTimer > 0)
            {
                Debug.Log("=> Saved by damage cooldown");
                return;
            }

            _damageCooldownTimer = m_DefaultDamageCooldownTime;
            // play damage fx
            // push back with physics.
            // m_Player.Rb.AddForce(knockbackDirection * m_HitKnockbackForce);
            m_Player.PlayerMovement.IsMovementEnabled = false;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
            m_Player.Rb.AddExplosionForce(m_HitKnockbackForce, hitPoint, 1f);
            // play damage animation.
            m_Player.Anim.SetTrigger(Constants.Animation.DAMAGE);
        }


#region Event lisetners
        
        private void OnItemPicked(InteractableObject obj)
        {
            _heldWeapon = obj as Weapon;
            if (HasWeapon)
                CurrentDamageAmount = _heldWeapon.Damage;
        }

        private void OnItemDropped(InteractableObject obj)
        {
            _heldWeapon = null;
            CurrentDamageAmount = m_SimpleAttackDamage;
        }

#endregion

        
#region Animation events

        /// <summary>
        /// Player attack anims
        /// </summary>
        public void AnimEvent_AttackBegin()
        {
            m_Player.PlayerMovement.IsMovementEnabled = false;
            
            // Activate PlayerAttackHitBoxes
            _playerHitboxes.Clear();
            if (HasWeapon)
                _playerHitboxes.Add(_heldWeapon.Hitbox);
            else
                for (int i = 0; i < m_DefaultHitboxes.Length; i++)
                    _playerHitboxes.Add(m_DefaultHitboxes[i]);

            for (int i = 0; i < _playerHitboxes.Count; i++)
                _playerHitboxes[i].SetActive(true);
        }

        public void AnimEvent_AttackEnd()
        {
            // Deactivate PlayerAttackHitBoxes
            for (int i = 0; i < _playerHitboxes.Count; i++)
                _playerHitboxes[i].SetActive(false);
            
            _playerHitboxes.Clear();
        }

        public void AnimEvent_ClipEnd()
        {
            DOTween.To(() => m_Player.Rig.weight, x => m_Player.Rig.weight = x, _cachedRigWeight, .01f);
            m_Player.PlayerMovement.IsMovementEnabled = true;
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
            _attackCooldownTimer = .1f;
        }

        /// <summary>
        /// Knockback anims
        /// </summary>
        public void AnimEvent_KnockbackBegin()
        {
            for (int i = 0; i < _playerHitboxes.Count; i++)
                _playerHitboxes[i].SetActive(false);
            
            m_Player.PlayerMovement.IsMovementEnabled = false;
            
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
            m_Player.PlayerInteraction.DropItem();              // Instantaneously allow player to Drop the item and disable PlayerInteractions.
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
        }

        public void AnimEvent_KnockbackGroundImpact()
        {
            // play dust particles
        }

        public void AnimEvent_KnockbackEnd()
        {
            m_Player.PlayerMovement.IsMovementEnabled = true;
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
        }

#endregion

    }
}
