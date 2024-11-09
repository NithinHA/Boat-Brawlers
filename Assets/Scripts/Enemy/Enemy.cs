using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BaseObjects.Enemy
{
    public class Enemy : BaseObject
    {
        [Header("Locomotion info")]
        [SerializeField] private float m_NextLeapWaitTime = 5f;
        [SerializeField] private float m_DirectTowardsPlayerDelay = .5f;
        [Header("Health info")]
        [SerializeField] private float m_MaxHealth = 100f;
        [SerializeField] private float m_FailedToLandDestroyTimer = 4f;
        [Header("Refs")]
        [SerializeField] private ParticleSystem m_ChargingParticles;
        [SerializeField] private GameObject m_DeathParticlesPrefab;         // make sure auto destroy is enabled for this
        [Space]
        [SerializeField] private Renderer m_Renderer;
        [SerializeField] internal Animator Anim;
        [SerializeField] internal Rigidbody Rb;
        [SerializeField] private EnemyAttack m_EnemyAttack;
        [Space]
        [SerializeField] private LayerMask m_GroundLayer;

        public float Health;
        public bool IsLanded = false;
        
        private float _leapTimer;
        private Sequence _chargingSequence = null;
        private float _timeSinceSpawn;

        public enum EnemyState
        {
            Idle, Charging, Attack
        }
        public EnemyState CurrentState;
        public Action OnStateChange;
        public static Action<Enemy> OnEnemyDestroyed;
        
        private readonly string[] _impactSounds = { Constants.SoundNames.ENEMY_IMPACT, Constants.SoundNames.ENEMY_IMPACT_1 };

#region Getters

        public Renderer GetRenderer() => m_Renderer;
        public LayerMask GetGroundLayer() => m_GroundLayer;

#endregion
        
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
                KillEnemy(EnemyDeathCause.Overboard);
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
                    m_ChargingParticles.gameObject.SetActive(true);
                    _chargingSequence.Append(transform.DORotateQuaternion(lookRotation, m_DirectTowardsPlayerDelay))
                        .AppendInterval(.5f)
                        .AppendCallback(() => m_ChargingParticles.gameObject.SetActive(false))
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
                KillEnemy(EnemyDeathCause.KIA);
                return;
            }

            string attackSound = _impactSounds[Random.Range(0, _impactSounds.Length)];
            AudioManager.Instance.PlaySound(attackSound);
            _leapTimer = m_NextLeapWaitTime;
            Anim.SetTrigger(Constants.Animation.DAMAGE);

            if(CurrentState == EnemyState.Charging)
                ResetFailedAttack();
        }

        private void KillEnemy(EnemyDeathCause cause)
        {
            if (CurrentState == EnemyState.Charging)
                ResetFailedAttack();

            switch (cause)
            {
                case EnemyDeathCause.KIA:
                    AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_DEATH);
                    break;
                case EnemyDeathCause.Overboard:
                    AudioManager.Instance.PlaySound(Constants.SoundNames.ENEMY_DEATH_OVERBOARD);
                    break;
            }
            Instantiate(m_DeathParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }

        private void HandlePreLanding()
        {
            if (_timeSinceSpawn < m_FailedToLandDestroyTimer)
                _timeSinceSpawn += Time.deltaTime;
            else
                KillEnemy(EnemyDeathCause.KIA);
        }

        private void ResetFailedAttack()
        {
            if (_chargingSequence != null)
                _chargingSequence.Kill();

            if (m_ChargingParticles.gameObject.activeSelf)
                m_ChargingParticles.gameObject.SetActive(false);

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

    public enum EnemyDeathCause
    {
        KIA, Overboard
    }
}