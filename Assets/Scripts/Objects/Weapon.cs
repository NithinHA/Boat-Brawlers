using DG.Tweening;
using UnityEngine;

namespace BaseObjects
{
    public class Weapon : InteractableObject
    {
        [Header("Weapon info")]
        public float AttackCooldown = 1f;
        public WeaponType Type;
        public int MaxComboIndex = 1;
        [SerializeField] private AtkAnimVariant[] m_AttackVariants;
        public AtkAnimInfo HeavyAttackAnimInfo;

        private float[] _variantWeights;

        protected override void Start()
        {
            base.Start();

            _variantWeights = new float[m_AttackVariants.Length];
            for (int i = 0; i < m_AttackVariants.Length; i++)
                _variantWeights[i] = m_AttackVariants[i].Weight;
        }

        public void Attack(int index)
        {
            AudioManager.Instance.PlaySound(Constants.SoundNames.WEAPON_SWING);
        }

        // public int GetClipIndex()
        // {
        //     return Utilities.GetWeightedRandomIndex(_variantWeights);
        // }

        public AtkAnimVariant ChooseVariant()
        {
            return m_AttackVariants[Utilities.GetWeightedRandomIndex(_variantWeights)];
        }

        public void OnHeavyAttack(GameObject particles)
        {
            Transform hitT = HeavyAttackAnimInfo.HitboxInfo.Hitboxes[0].transform;
            RaftController_Custom.Instance.AddInstantaneousForce(hitT.position, 1f);
            DOVirtual.DelayedCall(.05f, () =>
            {
                Vector3 pos = hitT.position;
                Instantiate(particles, pos, Quaternion.identity);
            });
        }

        // public HitboxInfo GetHitboxInfoForAttackIndex(int index)
        // {
        //     return m_AnimWeightHitboxMap[index].HitboxInfo;
        // }

        // public bool IsHeavyAttack(int attackIndex)
        // {
        //     return false;
        // }

    }

    public enum WeaponType
    {
        None, Hammer
    }
}
