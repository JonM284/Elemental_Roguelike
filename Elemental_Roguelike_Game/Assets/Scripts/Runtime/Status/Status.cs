using Data.Elements;
using Runtime.Character;
using Runtime.VFX;
using UnityEngine;

namespace Runtime.Status
{
    public abstract class Status: ScriptableObject
    {

        public string statusName;
        
        public int roundCooldownTimer;
        
        public ElementTyping abilityElement;

        public VFXPlayer statusStayVFX;
        
        public VFXPlayer statusOneTimeVFX;

        public bool playVFXOnTrigger;

        public abstract void TriggerStatusEffect(CharacterBase _character);

        public abstract void ResetStatusEffect(CharacterBase _character);

    }
}