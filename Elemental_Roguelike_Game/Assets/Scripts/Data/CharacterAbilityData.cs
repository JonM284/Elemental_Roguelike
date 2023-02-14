using Data;
using Data.Elements;
using Runtime.Abilities;
using UnityEngine;

namespace Project.Scripts.Data
{
    [CreateAssetMenu(menuName = "Custom Data/Ability Data")]
    public class CharacterAbilityData: ScriptableObject
    {
        #region Public Fields

        public string abilityName;

        //cooldowns are determined by rounds
        public int roundCooldownTimer;

        public float baseDamage;

        public ElementTyping abilityElement;

        public float criticalMultiplier;

        public bool hasPrerequisites;

        public Ability ability;

        #endregion

    }
}