using UnityEngine;

namespace BaseObjects
{
    public class Weapon : InteractableObject
    {
        [Header("Weapon info")]
        public float AttackCooldown = 1f;
        public WeaponType Type;
        public int AttackAnimVarities = 1;
        
        public void Attack()
        {
            
        }
    }

    public enum WeaponType
    {
        None, Hammer
    }
}
