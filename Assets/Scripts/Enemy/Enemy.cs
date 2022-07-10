using System;
using System.Collections;
using System.Collections.Generic;
using BaseObjects.Player;
using UnityEngine;

namespace BaseObjects.Enemy
{
    public class Enemy : BaseObject
    {
        [Header("Health info")]
        [SerializeField] private float m_MaxHealth = 100f;
        [SerializeField] private GameObject m_EnemyHitbox;
        [SerializeField] private float m_HitKnockbackForce = 500f;
        [Header("Refs")]
        [SerializeField] protected Animator Anim;
        [SerializeField] protected Rigidbody Rb;

        public float Health;
        private bool _isLanded = false;
        private bool _attackInProgress = false;

#region Unity callbacks

        protected override void Start()
        {
            Health = m_MaxHealth;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
                Landed();
            else if (Input.GetKeyDown(KeyCode.N))
                Attack();
            else if (Input.GetKeyDown(KeyCode.M))
                TakeDamage(1, transform.position + transform.forward);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isLanded)
            {
                if (collision.gameObject.CompareTag(Constants.Tags.RAFT))
                    Landed();

                return;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(Constants.Tags.PLAYER_ATTACK_HITBOX))
            {
                PlayerAttack playerAttack = other.gameObject.GetComponentInParent<PlayerAttack>();
                TakeDamage(playerAttack.CurrentDamageAmount, other.ClosestPoint(transform.position));
            }
        }

#endregion

        private void Landed()
        {
            _isLanded = true;
            Anim.SetTrigger(Constants.Animation.LANDED);
            base.Start();
        }

        private void Attack()
        {
            Anim.SetTrigger(Constants.Animation.ATTACK);
        }

        public void TakeDamage(float amount, Vector3 hitPoint)
        {
            Anim.SetTrigger(Constants.Animation.DAMAGE);
            // push back
            Rb.AddExplosionForce(amount * m_HitKnockbackForce, hitPoint, 1f);

            Health -= amount;
            CheckDeath();
        }

        private void CheckDeath()
        {
            
        }

        private void KillEnemy()
        {
            
        }

        public void EnemyOverboard()
        {
            
        }


#region Animation events

        public virtual void AnimEvent_AttackBegin()
        {
            _attackInProgress = true;
            m_EnemyHitbox.SetActive(true);
        }

        public virtual void AnimEvent_AttackEnd()
        {
            _attackInProgress = false;
            m_EnemyHitbox.SetActive(false);
        }

        public void AnimEvent_KnockbackGroundImpact()
        {
            // play dust particles
        }

        public void AnimEvent_KnockbackEnd()
        {
            if (_attackInProgress)
                m_EnemyHitbox.SetActive(false);
        }

#endregion

    }
}