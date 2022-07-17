using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BaseObjects
{
    public class Weapon : InteractableObject
    {
        [Header("Weapon info")]
        public float AttackCooldown = 1f;
        public float Damage = 40f;
        public WeaponType Type;
        [SerializeField] private AnimWeightHitboxMap[] m_AnimWeightHitboxMap;
        public int HeavyAttackIndex = 0;

        private float[] _animWeights;

        [SerializeField] private float _instantaneousAttackWeight = 50f;

        protected override void Start()
        {
            base.Start();

            _animWeights = new float[m_AnimWeightHitboxMap.Length];
            for (int i = 0; i < m_AnimWeightHitboxMap.Length; i++)
                _animWeights[i] = m_AnimWeightHitboxMap[i].Weight;
        }

        public void Attack(int index)
        {
            AudioManager.Instance.PlaySound(Constants.SoundNames.WEAPON_SWING);
            if (index == HeavyAttackIndex)
            {
                // RaftController.Instance.AddInstantaneousForce(transform.position, _instantaneousAttackWeight);
            }
        }

        public int GetClipIndex()
        {
            return Utilities.GetWeightedRandomIndex(_animWeights);
        }

        public HitboxInfo GetHitboxInfoForAttackIndex(int index)
        {
            return m_AnimWeightHitboxMap[index].HitboxInfo;
        }
    }

    public enum WeaponType
    {
        None, Hammer
    }
}
