using System;
using UnityEngine;
using VFX;

namespace BaseObjects.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Enemy m_Enemy;
        [SerializeField] private GameObject m_EnemyHitbox;
        [SerializeField] private Vector2 m_HitKnockbackRange = new Vector2(200, 350);
        [Space]
        [SerializeField] private float m_LeapMagnitude = 6f;
        [SerializeField] private float m_BlinkDuration = .5f;
        [Space]
        [SerializeField] private ParticleSystem m_DamageParticles;
        [SerializeField] private GameObject m_GroundImpactParticles;
        [SerializeField] private Vector2 m_CamShakeEnemyKnockbackRange = new Vector2(3, 6);

        private Action _onAttackAnimComplete;
        private Vector3 _attackDir;

#region Unity callbacks

        private Vector3 _knockbackDir;
        private float _knockbackMag;
        private void OnTriggerEnter(Collider other)
        {
            if (!m_Enemy.IsLanded)
                return;

            if (other.CompareTag(Constants.Tags.PLAYER_ATTACK_HITBOX))
            {
                Vector3 knockBackDirection = GetCorrectedKnockBackDirection();
                _knockbackDir = knockBackDirection.normalized;
                TakeDamage(Player.Player.Instance.PlayerAttack.CurrentDamageAmount, knockBackDirection.normalized);
            }
        }

#endregion

#region Attack

        public void Attack(Action onComplete = null)
        {
            _attackDir = GetAttackDirection();
            m_Enemy.Rb.AddForce(_attackDir * m_LeapMagnitude, ForceMode.Impulse);
            _onAttackAnimComplete = onComplete;
        }

        Vector3 GetAttackDirection()
        {
            return Physics.Raycast(transform.position, Vector3.down, out var hit, 1f, m_Enemy.GetGroundLayer())
                ? Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized      // align the direction to the slope of Ground
                : transform.forward;        // error handler case when Raycast does not hit GroundLayer
        }

#endregion

#region Take Damage

        private bool _isCounterAttack;
        public void TakeDamage(float amount, Vector3 knockbackDirection)
        {
            _isCounterAttack = m_Enemy.CurrentState == Enemy.EnemyState.Attack;     // checks if the enemy was in middle of an attack when the player countered.

            _knockbackMag = _isCounterAttack ? 550 : Mathf.Clamp(Utilities.Remap(amount, new Vector2(0, 100), m_HitKnockbackRange),
                m_HitKnockbackRange.x, m_HitKnockbackRange.y);
            m_Enemy.Rb.AddForce(knockbackDirection * _knockbackMag);
            // particles
            m_DamageParticles.gameObject.SetActive(true);
            m_DamageParticles.Play();
            // perform cooldown
            ObjectBlinkHandler.Instance.BlinkOnce(m_Enemy.GetRenderer(), Color.red, m_BlinkDuration, 20);
            // camera shake
            float remappedValueForCameraShake = _isCounterAttack ? 8 :
                Mathf.Clamp(Utilities.Remap(amount, new Vector2(0, 100), m_CamShakeEnemyKnockbackRange),
                    m_CamShakeEnemyKnockbackRange.x, m_CamShakeEnemyKnockbackRange.y);
            Debug.Log($"=> camShake: {remappedValueForCameraShake}; knockback: {_knockbackMag}");
            CameraHolder.Instance.TriggerCameraShake(.2f, remappedValueForCameraShake, .15f);
            // frame freeze
            FrameFreezeHandler.Instance.PerformFrameFreeze(.05f, _isCounterAttack ? .18f: .14f);
            if (m_Enemy.CurrentState == Enemy.EnemyState.Attack)
            {
                if(m_EnemyHitbox.activeSelf)
                    m_EnemyHitbox.SetActive(false);

                _onAttackAnimComplete?.Invoke();
                _onAttackAnimComplete = null;
            }

            m_Enemy.OnTakeDamage(amount);
        }

        /// <summary>
        /// Returns the enemy knockback direction considering the plane angle.
        /// </summary>
        private Vector3 GetCorrectedKnockBackDirection()
        {
            Vector3 position = transform.position;
            Vector3 knockBackDir = position - Player.Player.Instance.transform.position;    // cur_pos - player_pos = base knockback direction
            return Physics.Raycast(position, Vector3.down, out var hit, 1f, m_Enemy.GetGroundLayer())
                ? Vector3.ProjectOnPlane(knockBackDir, hit.normal).normalized       // align the direction to the slope of Ground
                : knockBackDir;        // error handler case when Raycast does not hit GroundLayer
        }

#endregion

#region Animation events

        public virtual void AnimEvent_AttackBegin()
        {
            m_EnemyHitbox.SetActive(true);
            AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_LEAP);

        }

        public virtual void AnimEvent_AttackEnd()
        {
            m_EnemyHitbox.SetActive(false);
        }

        public void AnimEvent_AttackComplete()
        {
            _onAttackAnimComplete?.Invoke();
            _onAttackAnimComplete = null;
        }

        public void AnimEvent_KnockbackGroundImpact()
        {
            // play particles
            // play sfx
        }

        public void AnimEvent_KnockbackEnd()
        {
            if(m_EnemyHitbox.activeSelf)
                m_EnemyHitbox.SetActive(false);
        }

#endregion
        
#region Draw

        void OnDrawGizmos()
        {
            Vector3 enemyPosition = transform.position;
            Gizmos.color = Color.yellow;
            
            Gizmos.DrawLine(enemyPosition, enemyPosition + _knockbackDir * _knockbackMag);       // Draw a line representing the attack direction
            DrawArrowHead(enemyPosition + _knockbackDir * _knockbackMag, _knockbackDir);    // Draw an arrowhead at the end of the attack direction line (for better visualization)
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(enemyPosition, enemyPosition + _attackDir * m_LeapMagnitude);       // Draw a line representing the attack direction
            DrawArrowHead(enemyPosition + _attackDir * m_LeapMagnitude, _attackDir);    // Draw an arrowhead at the end of the attack direction line (for better visualization)
        }

        private void DrawArrowHead(Vector3 position, Vector3 direction)
        {
            float arrowHeadLength = 0.5f;
            float arrowHeadAngle = 20.0f;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

            Gizmos.DrawLine(position, position + right * arrowHeadLength);
            Gizmos.DrawLine(position, position + left * arrowHeadLength);
        }

#endregion

    }
}