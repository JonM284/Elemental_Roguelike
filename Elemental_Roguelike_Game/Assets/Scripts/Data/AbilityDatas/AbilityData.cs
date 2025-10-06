using System.Collections.Generic;
using Data.StatusDatas;
using Runtime.Abilities;
using Runtime.VFX;
using UnityEngine;

namespace Data.AbilityDatas
{
    public class AbilityData: ScriptableObject
    {
        [Header("Visuals / Description")] 
        public string abilityName = "==== Ability Name ====";
        
        [TextArea(1,4)]
        public string abilityDescription = "//// Ability Description ////";

        public string abilityGUID;
        
        public AbilityType abilityType;
        public AbilityTargetType targetType;
        
        public Sprite abilityIconRef;
        
        public AnimationClip abilityAnimationOverride;

        public bool playVFXAtTransform;
        
        [Header("Gameplay")]
        public int abilityTurnCooldownTimer = 1;

        public int abilityDamageAmount = 1;
        public float abilityKnockbackAmount = 1f;
        public float abilityRange = 1f, abilityScale = 1f;
        
        public VFXPlayer abilityVFX;

        public List<AudioClip> abilityUseSFX = new List<AudioClip>();
        
        public List<AbilityCategories> abilityCategories = new List<AbilityCategories>();
        
        [Header("Status'")]
        public List<StatusData> applicableStatusesOnHit = new List<StatusData>();
        
        [Header("Reference")]
        public GameObject abilityGameObject;

        [ContextMenu("Generate GUID")]
        private void GenerateID()
        {
            abilityGUID = System.Guid.NewGuid().ToString();
        }

    }
}