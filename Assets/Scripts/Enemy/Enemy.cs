using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using BaseObjects.Player;
using Unity.Mathematics;

namespace BaseObjects.Enemy
{
    public class Enemy : BaseObject
    {
        [Header("Locomotion info")]
        [SerializeField] private float m_NextLeapWaitTime = 5f;
        [SerializeField] private float m_LeapMagnitude = 2f;
        [SerializeField] private float m_DirectTowardsPlayerDelay = .5f;
        [Header("Health info")]
        [SerializeField] private float m_MaxHealth = 100f;
        [SerializeField] private float m_FailedToLandDestroyTimer = 4f;

        [Header("Refs")]
        [SerializeField] private GameObject m_ChargingParticlesPrefab;
        [SerializeField] private GameObject m_DeathParticlesPrefab;         // make sure auto destroy is enabled for this
        [Space]
        [SerializeField] internal Animator Anim;
        [SerializeField] internal Rigidbody Rb;
        [SerializeField] private EnemyAttack m_EnemyAttack;
        [Space]

        public float Health;
        public bool IsLanded = false;
        
        private float _leapTimer;
        private Sequence _chargingSequence = null;
        private GameObject _chargingParticles;
        private float _timeSinceSpawn;

        public enum EnemyState
        {
            Idle, Charging, Attack
        }
        public EnemyState CurrentState;
        public Action OnStateChange;
        public static Action<Enemy> OnEnemyDestroyed;

#region Unity callbacks

        protected override void Start()
        {
            Health = m_MaxHealth;
        }

        protected override void OnDestroy()
        {
            OnEnemyDestroyed?.Invoke(this);
            base.OnDestroy();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsLanded)
            {
                if (collision.gameObject.CompareTag(Constants.Tags.RAFT))
                    Landed();

                return;
            }
        }

        private void Update()
        {
            if (!IsLanded)
            {
                HandlePreLanding();
                return;
            }
            
#if UNITY_EDITOR
            KeyboardShortcuts();
#endif

            if (transform.position.y < -5f)
            {
                KillEnemy();
            }

            switch (CurrentState)
            {
                case EnemyState.Idle:
                    if (_leapTimer > 0)
                    {
                        _leapTimer -= Time.deltaTime;
                        return;
                    }

                    ChangeState(EnemyState.Charging);
                    
                    Quaternion lookRotation = GetRotationToPlayer();
                    _chargingSequence = DOTween.Sequence();
                    _chargingParticles = Instantiate(m_ChargingParticlesPrefab, transform.position, Quaternion.identity);   // create charging particles
                    _chargingSequence.Append(transform.DORotateQuaternion(lookRotation, m_DirectTowardsPlayerDelay))
                        .AppendInterval(.5f)
                        .AppendCallback(() =>
                        {
                            if (_chargingParticles != null)
                            {
                                Destroy(_chargingParticles);
                                _chargingParticles = null;
                            }
                        })
                        .AppendCallback(Attack);
                    break;
            }
        }

        private void KeyboardShortcuts()
        {
            if (Input.GetKeyDown(KeyCode.B))
                Landed();
            else if (Input.GetKeyDown(KeyCode.N))
                Attack();
            else if (Input.GetKeyDown(KeyCode.M))
                m_EnemyAttack.TakeDamage(1, (transform.position - Player.Player.Instance.transform.position).normalized);
        } 

#endregion

        public void ChangeState(EnemyState newState)
        {
            CurrentState = newState;
            OnStateChange?.Invoke();
        }
        
        private void Landed()
        {
            IsLanded = true;
            Anim.SetTrigger(Constants.Animation.LANDED);
            _leapTimer = m_NextLeapWaitTime;
            base.Start();

            AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_LAND);
            // play particles
        }

        private void Attack()
        {
            ChangeState(EnemyState.Attack);
            Anim.SetTrigger(Constants.Animation.ATTACK);
            Rb.AddForce(transform.forward * m_LeapMagnitude, ForceMode.Impulse);
            m_EnemyAttack.Attack(() =>
            {
                _leapTimer = m_NextLeapWaitTime;
                ChangeState(EnemyState.Idle);
                Rb.velocity = Vector3.zero;
            });
        }

        internal void OnTakeDamage(float amount)
        {
            Health -= amount;
            if (Health <= 0)
            {
                KillEnemy();
                AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_DEATH);      // play EnemyDeath effect only when killed by player on Raft.
                return;
            }

            AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_IMPACT);
            _leapTimer = m_NextLeapWaitTime;
            Anim.SetTrigger(Constants.Animation.DAMAGE);

            if(CurrentState == EnemyState.Charging)
                ResetFailedAttack();
        }

        private void KillEnemy()
        {
            if (CurrentState == EnemyState.Charging)
                ResetFailedAttack();

            Instantiate(m_DeathParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }

        private void HandlePreLanding()
        {
            if (_timeSinceSpawn < m_FailedToLandDestroyTimer)
                _timeSinceSpawn += Time.deltaTime;
            else
                KillEnemy();
        }

        private void ResetFailedAttack()
        {
            if (_chargingSequence != null)
                _chargingSequence.Kill();

            if (_chargingParticles != null)
            {
                Destroy(_chargingParticles);
                _chargingParticles = null;
            }
            ChangeState(EnemyState.Idle);
        }

        private Quaternion GetRotationToPlayer()
        {
            Vector3 playerDir = Player.Player.Instance.transform.position - transform.position;
            playerDir.y = 0;
            playerDir.Normalize();
            return Quaternion.LookRotation(playerDir);
        }

    }
}
