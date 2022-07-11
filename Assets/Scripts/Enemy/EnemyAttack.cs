using System;
using System.Collections;
using System.Collections.Generic;
using BaseObjects.Player;
using UnityEngine;

namespace BaseObjects.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Enemy m_Enemy;
        [SerializeField] private GameObject m_EnemyHitbox;
        [SerializeField] private float m_HitKnockbackForce = 20f;

        private bool _isLanded = false;
        private Action _onAttackAnimComplete;

#region Unity callbacks

        private void OnTriggerEnter(Collider other)
        {
            if (!m_Enemy.IsLanded)
                return;

            if (other.CompareTag(Constants.Tags.PLAYER_ATTACK_HITBOX))
            {
                Vector3 knockbackDirection = transform.position - Player.Player.Instance.transform.position;
                knockbackDirection.y = 0;
                TakeDamage(Player.Player.Instance.PlayerAttack.CurrentDamageAmount, knockbackDirection.normalized);
            }
        }

#endregion

        public void Attack(Action onComplete = null)
        {
            _onAttackAnimComplete = onComplete;
        }

        public void TakeDamage(float amount, Vector3 knockbackDirection)
        {
            // push back
            m_Enemy.Rb.AddForce(knockbackDirection * amount * m_HitKnockbackForce);
            if (m_Enemy.CurrentState == Enemy.EnemyState.Attack)
            {
                if(m_EnemyHitbox.activeSelf)
                    m_EnemyHitbox.SetActive(false);

                _onAttackAnimComplete?.Invoke();
                _onAttackAnimComplete = null;
            }

            m_Enemy.OnTakeDamage(amount);
        }


#region Animation events

        public virtual void AnimEvent_AttackBegin()
        {
            m_EnemyHitbox.SetActive(true);
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
            // play dust particles
        }

        public void AnimEvent_KnockbackEnd()
        {
            if(m_EnemyHitbox.activeSelf)
                m_EnemyHitbox.SetActive(false);
        }

#endregion

    }
}