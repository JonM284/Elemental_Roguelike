using Data.Elements;
using UnityEngine;

namespace Runtime.Damage
{
    public interface IDamageable
    {
        public void OnRevive();
        
        public void OnDealDamage(Transform attacker, int _damageAmount, bool _armorPiercing ,ElementTyping _damageElementType, bool _hasKnockback);
    }
}