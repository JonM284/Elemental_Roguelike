using Data.Elements;
using Runtime.Character;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Status
{
    public abstract class Status: ScriptableObject
    {

        public string statusName;
        
        public int roundCooldownTimer;
        
        public ElementTyping abilityElement;

        public VFXPlayer statusVFX;

        //does the ability only trigger on impact?
        public bool isImpactOnlyStatus;

        public abstract void TriggerStatusEffect(CharacterBase _character);

    }
}