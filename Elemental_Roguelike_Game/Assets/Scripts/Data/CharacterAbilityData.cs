using Runtime.Abilities;
using UnityEngine;

namespace Project.Scripts.Data
{
    [CreateAssetMenu(menuName = "Custom Data/Ability Data")]
    public class CharacterAbilityData
    {
        #region Public Fields

        //cooldowns are determined by rounds
        public int roundCooldownTimer;

        public float baseDamage;

        public float criticalMultiplier;

        public bool hasPrerequisites;

        public Ability ability;

        #endregion

    }
}