using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BaseObjects.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private Player m_Player;

        [Header("Attack info")]
        [SerializeField] private float m_DefaultAttackCooldownTime = .5f;
        [SerializeField] private AnimWeightHitboxMap[] m_AnimWeightHitboxMap;
        [SerializeField] private float m_SimpleAttackDamage = 20f;

        private float _attackCooldownTimer = 0f;
        private bool _canAttack = true;
        private Weapon _heldWeapon;
        public bool HasWeapon => _heldWeapon != null;
        private float[] _animWeights;
        private List<GameObject> _playerHitboxes = new List<GameObject>();
        public float CurrentDamageAmount;
        private int _curAttackIndex;

        [Header("Damage info")]
        [SerializeField] private float m_HitKnockbackForce = 400f;
        [SerializeField] private float m_DefaultDamageCooldownTime = 4f;
        [SerializeField] private GameObject m_GroundImpactParticles;
        [SerializeField] private Vector2 m_CamShakePlayerFall = new Vector2(.5f, 1.5f);

        private float _damageCooldownTimer = 0f;

        private float _cachedRigWeight;


#region Unity callbacks

        private void Start()
        {
            _animWeights = new float[m_AnimWeightHitboxMap.Length];
            for (int i = 0; i < m_AnimWeightHitboxMap.Length; i++)
                _animWeights[i] = m_AnimWeightHitboxMap[i].Weight;

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
                TakeDamage(-transform.forward.normalized);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Constants.Tags.ENEMY_ATTACK_HITBOX))
            {
                Vector3 knockbackDirection = transform.position - other.transform.position;
                knockbackDirection.y = 0;
                TakeDamage(knockbackDirection.normalized);
                // enemy.HitSuccess();
            }
        }

#endregion

        public void Attack()
        {
            if (_attackCooldownTimer > 0 || !_canAttack)
            {
                Debug.Log("=> wait for attack cooldown");
                return;
            }

            _attackCooldownTimer = !HasWeapon ? m_DefaultAttackCooldownTime : _heldWeapon.AttackCooldown;
            _canAttack = false;
            _cachedRigWeight = m_Player.Rig.weight;
            m_Player.Rig.weight = 0;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;

            if (!HasWeapon)
            {
                SimpleAttack();
            }
            else
            {
                WeaponAttack();
            }
        }

        
        private void SimpleAttack()
        {
            _curAttackIndex = Utilities.GetWeightedRandomIndex(_animWeights);
            AnimateAttack(0, _curAttackIndex);
        }

        private void WeaponAttack()
        {
            _curAttackIndex = _heldWeapon.GetClipIndex();
            AnimateAttack((int) _heldWeapon.Type, _curAttackIndex);
            _heldWeapon.Attack(_curAttackIndex);
        }

        private void AnimateAttack(int weaponIndex, int attackIndex)
        {
            m_Player.Anim.SetInteger(Constants.Animation.WEAPON_INDEX, weaponIndex);
            m_Player.Anim.SetInteger(Constants.Animation.ATTACK_INDEX, attackIndex);
            m_Player.Anim.SetTrigger(Constants.Animation.ATTACK);
        }

        public void TakeDamage(Vector3 knockbackDirection)
        {
            if (_damageCooldownTimer > 0)
            {
                Debug.Log("=> Saved by damage cooldown");
                return;
            }

            _damageCooldownTimer = m_DefaultDamageCooldownTime;
            // play damage fx
            m_Player.PlayerMovement.IsMovementEnabled = false;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
            CameraShake.ShakeOnce(m_CamShakePlayerFall.x, m_CamShakePlayerFall.y);

            // push back
            m_Player.Rb.AddForce(knockbackDirection * m_HitKnockbackForce);
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

            ResetCurrentHitboxes();
            HitboxInfo hitboxInfo = !HasWeapon ? m_AnimWeightHitboxMap[_curAttackIndex].HitboxInfo : _heldWeapon.GetHitboxInfoForAttackIndex(_curAttackIndex);
            foreach (Collider col in hitboxInfo.Hitboxes)
            {
                SphereCollider hitbox = col as SphereCollider;
                if (hitbox == null)
                {
                    Debug.LogError(col.gameObject.name + " is not a SphereCollider");
                    return;
                }
                hitbox.gameObject.SetActive(true);
                _playerHitboxes.Add(hitbox.gameObject);
                hitbox.radius = hitboxInfo.Radius;
            }
        }

        public void AnimEvent_AttackEnd()
        {
            ResetCurrentHitboxes();
        }

        public void AnimEvent_ClipEnd()
        {
            DOTween.To(() => m_Player.Rig.weight, x => m_Player.Rig.weight = x, _cachedRigWeight, .01f);
            ResetPostAttackCompletion();
        }

        /// <summary>
        /// Knockback anims
        /// </summary>
        public void AnimEvent_KnockbackBegin()
        {
            ResetCurrentHitboxes();
            
            m_Player.PlayerMovement.IsMovementEnabled = false;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
        }

        public void AnimEvent_KnockbackGroundImpact()
        {
            ResetCurrentHitboxes();         // as a safety measure. Sometimes hitboxes will be enabled even after attack fails and knockback occurs.
            
            Instantiate(m_GroundImpactParticles, transform.position, Quaternion.identity);
            // play sfx
            // camera shake
            CameraShake.ShakeOnce(m_CamShakePlayerFall.x, m_CamShakePlayerFall.y);
            
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
            m_Player.PlayerInteraction.DropItem();              // Instantaneously allow player to Drop the item and disable PlayerInteractions.
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
        }

        public void AnimEvent_KnockbackEnd()
        {
            ResetPostAttackCompletion();
        }

#endregion

        private void ResetCurrentHitboxes()
        {
            for (int i = 0; i < _playerHitboxes.Count; i++)
                _playerHitboxes[i].SetActive(false);
            
            _playerHitboxes.Clear();
        }

        private void ResetPostAttackCompletion()
        {
            m_Player.PlayerMovement.IsMovementEnabled = true;
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
            _canAttack = true;
        }

    }
}

[System.Serializable]
public class AnimWeightHitboxMap
{
    public string ClipName;
    public float Weight;
    public HitboxInfo HitboxInfo;
}

[System.Serializable]
public class HitboxInfo
{
    public Collider[] Hitboxes;
    public float Radius;
}
