using UnityEngine;

namespace BaseObjects
{
    public class Weapon : InteractableObject
    {
        [Header("Weapon info")]
        public float AttackCooldown = 1f;
        public float Damage = 40f;
        public WeaponType Type;
        public GameObject Hitbox;
        [SerializeField] private float[] m_AnimWeights;

        public void Attack()
        {
            
        }

        public int GetClipIndex()
        {
            return Utilities.GetWeightedRandomIndex(m_AnimWeights);
        }
    }

    public enum WeaponType
    {
        None, Hammer
    }
}
