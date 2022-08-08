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

        [SerializeField] private AtkAnimVariant[] m_AttackVariants;
        // [SerializeField] private AtkAnimInfo[] m_AnimWeightHitboxMap;
        [SerializeField] private float m_SimpleAttackDamage = 20f;

        private float _attackCooldownTimer = 0f;
        private bool _canAttack = true;
        private Weapon _heldWeapon;
        public bool HasWeapon => _heldWeapon != null;
        private float[] _variantWeights;
        private List<GameObject> _playerHitboxes = new List<GameObject>();
        public float CurrentDamageAmount;

        // combo data
        private bool _isPerformingCombo = false;
        private int _simpleAttackMaxComboIndex = 1;
        private int _currentComboMax = 0;
        private int _currentComboIndex = 0;
        private AtkAnimVariant _selectedComboVariant = null;

        [Header("Damage info")]
        [SerializeField] private float m_HitKnockbackForce = 400f;
        [SerializeField] private float m_DefaultDamageCooldownTime = 4f;
        [SerializeField] private GameObject m_GroundImpactParticles;
        [SerializeField] private Vector2 m_CamShakePlayerFall = new Vector2(.5f, 1.5f);

        private float _damageCooldownTimer = 0f;
        private bool _isPlayingDamageAnim = false;
        private float _cachedRigWeight;


#region Unity callbacks

        private void Start()
        {
            _variantWeights = new float[m_AttackVariants.Length];
            for (int i = 0; i < m_AttackVariants.Length; i++)
                _variantWeights[i] = m_AttackVariants[i].Weight;

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
            else if (Input.GetKeyDown(KeyCode.J))
                Attack(true);
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
            }
        }

#endregion

        private bool _isChargedAttacking;
        public void Attack(bool isChargedAttack = false)
        {
            if (_isPlayingDamageAnim && _isChargedAttacking)
                return;

            if (_currentComboIndex > _currentComboMax)      // Bugfix: If player performs 2nd attack at the time interval between AnimEvents- AttackEnd() & ClipEnd(), the player freezes as AttackEnd() returns thinking Combo has ended, while ClipEnd() returns thinking Combo is still going on.
                return;

            if ((_attackCooldownTimer > 0 || !_canAttack) && !isChargedAttack)
            {
                _isPerformingCombo = true;
                _currentComboMax = Mathf.Clamp(++_currentComboMax, 0, !HasWeapon ? _simpleAttackMaxComboIndex : _heldWeapon.MaxComboIndex);
                return;
            }

            _isChargedAttacking = isChargedAttack;
            _attackCooldownTimer = !HasWeapon ? m_DefaultAttackCooldownTime : _heldWeapon.AttackCooldown;
            _canAttack = false;
            if (!_isPerformingCombo)
            {
                _cachedRigWeight = m_Player.Rig.weight;
            }
            m_Player.Rig.weight = 0;
            m_Player.PlayerInteraction.IsInteractionEnabled = false;

            if (!HasWeapon)
            {
                SimpleAttack();
            }
            else
            {
                WeaponAttack(isChargedAttack);
            }
        }

        private void SimpleAttack()
        {
            _selectedComboVariant ??= m_AttackVariants[Utilities.GetWeightedRandomIndex(_variantWeights)];
            int animIndex = _selectedComboVariant.AllAnimInfos[_currentComboIndex].AnimIndex;
            AnimateAttack(0, animIndex);
            AudioManager.Instance.PlaySound(Constants.SoundNames.PLAYER_KICK);
        }

        private void WeaponAttack(bool isChargedAttack = false)
        {
            int animIndex = 0;
            if (isChargedAttack)
            {
                _selectedComboVariant = new AtkAnimVariant(_heldWeapon.HeavyAttackAnimInfo);
                animIndex = _selectedComboVariant.AllAnimInfos[0].AnimIndex;
            }
            else
            {
                _selectedComboVariant ??= _heldWeapon.ChooseVariant();
                animIndex = _selectedComboVariant.AllAnimInfos[_currentComboIndex].AnimIndex;
            }
            AnimateAttack((int) _heldWeapon.Type, animIndex);
            _heldWeapon.Attack(_currentComboIndex);
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
            
            _isPlayingDamageAnim = true;
            // resets
            if(_isPerformingCombo)
                ResetCombos();
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
            if (_selectedComboVariant == null)
                Debug.LogError("=> Variant null!");
            HitboxInfo hitboxInfo = _selectedComboVariant.AllAnimInfos[_currentComboIndex].HitboxInfo;

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

        public void AnimEvent_HeavyAttackImpact()
        {
            AudioManager.Instance.PlaySound(Constants.SoundNames.HAMMER_SMASH);
            // play particles impact
            RaftController_Custom.Instance.AddInstantaneousForce(_heldWeapon.transform.position, 1f);
        }

        public void AnimEvent_AttackEnd()
        {
            ResetCurrentHitboxes();

            _currentComboIndex++;
            if (_currentComboIndex <= _currentComboMax)
            {
                m_Player.PlayerMovement.IsMovementEnabled = true;
                _canAttack = true;
                _attackCooldownTimer = 0;
                Attack();
            }
        }

        public void AnimEvent_ClipEnd()
        {
            if (_currentComboIndex <= _currentComboMax)
                return;

            ResetCombos();
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
            AudioManager.Instance.PlaySound(Constants.SoundNames.PLAYER_FALL);
            // camera shake
            CameraShake.ShakeOnce(m_CamShakePlayerFall.x, m_CamShakePlayerFall.y);
            
            m_Player.PlayerInteraction.IsInteractionEnabled = true;
            m_Player.PlayerInteraction.DropItem();              // Instantaneously allow player to Drop the item and disable PlayerInteractions.
            m_Player.PlayerInteraction.IsInteractionEnabled = false;
        }

        public void AnimEvent_KnockbackEnd()
        {
            _isPlayingDamageAnim = false;
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

        private void ResetCombos()
        {
            _isPerformingCombo = false;
            _currentComboMax = 0;
            _currentComboIndex = 0;
            _selectedComboVariant = null;

            if (_isChargedAttacking)
                _isChargedAttacking = false;
        }
    }
}

[System.Serializable]
public class AtkAnimVariant
{
    public AtkAnimInfo[] AllAnimInfos;
    public float Weight;

    public AtkAnimVariant(AtkAnimInfo singleAttack)
    {
        AllAnimInfos = new AtkAnimInfo[] {singleAttack};
        Weight = 1;
    }
}

[System.Serializable]
public class AtkAnimInfo
{
    public string ClipName;
    public int AnimIndex;
    public HitboxInfo HitboxInfo;
}

[System.Serializable]
public class HitboxInfo
{
    public Collider[] Hitboxes;
    public float Radius;
}
