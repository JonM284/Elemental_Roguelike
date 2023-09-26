using Data.Elements;
using Runtime.Character;
using Runtime.VFX;
using UnityEngine;

namespace Runtime.Status
{
    public abstract class Status: ScriptableObject
    {

        [Header("Status Common")]
        
        public string statusName;
        
        public int roundCooldownTimer;

        [Header("Chance and Element")]
        
        [Tooltip("Chance to Remove this status per round 0 - 100, if 0: will not try to remove")]
        [Range(0,100)]
        public int chanceToRemove;
        
        public ElementTyping abilityElement;

        public StatusType statusType;

        [Header("VFX")]
        
        public VFXPlayer statusStayVFX;
        
        public VFXPlayer statusOneTimeVFX;

        public bool playVFXOnTrigger;

        [Header("Icons")]
        
        public Sprite statusIcon;

        public Sprite statusIconLow;

        public abstract void TriggerStatusEffect(CharacterBase _character);

        public abstract void ResetStatusEffect(CharacterBase _character);

    }
}