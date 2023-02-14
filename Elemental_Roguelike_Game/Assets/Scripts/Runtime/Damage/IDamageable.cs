using Data.Elements;

namespace Runtime.Damage
{
    public interface IDamageable
    {
        public void OnDealDamage(int _damageAmount, bool _armorPiercing ,ElementTyping _damageElementType);
    }
}