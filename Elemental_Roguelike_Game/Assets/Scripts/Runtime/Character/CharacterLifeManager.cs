using System;
using Data;
using Data.Elements;
using Project.Scripts.Data;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Events

        public static event Action OnCharacterDied;

        public static event Action OnCharacterHealed;
        

        #endregion

        #region Serialized Fields


        #endregion

        #region Accessors
        
        public int currentHealthPoints { get; private set; }

        public int currentShieldPoints { get; private set; } 

        public int maxHealthPoints { get; private set; }

        public int maxSheildPoints { get; private set; }
        
        public ElementTyping characterElementType { get; private set; }

        public bool isAlive => currentHealthPoints > 0;

        #endregion

        #region Class Implementation

        public void InitializeCharacterHealth(int _maxHealth, int _maxShields, int _currentHealth, int _currentShield, ElementTyping _typing)
        {
            maxHealthPoints = _maxHealth;
            maxSheildPoints = _maxShields;
            currentHealthPoints = _currentHealth;
            currentShieldPoints = _currentShield;
            characterElementType = _typing;
        }

        public void DealDamage(int _incomingDamage, bool _armorPiercing, ElementTyping _type)
        {
            var _fixedIncomingDamage = characterElementType.CalculateDamageOnWeakness(_incomingDamage, _type);
            
            if (_armorPiercing)
            {
                currentHealthPoints -= _fixedIncomingDamage;
                return;
            }

            if (_incomingDamage >= currentShieldPoints)
            {
                currentShieldPoints = 0;
                currentHealthPoints -= (_fixedIncomingDamage - currentShieldPoints);
            }
            else
            {
                currentShieldPoints -= _fixedIncomingDamage;
            }

            if (currentHealthPoints <= 0)
            {
                OnCharacterDied?.Invoke();
            }
        }

        public void FullReviveCharacter()
        {
            currentHealthPoints = maxHealthPoints;
            currentShieldPoints = maxSheildPoints;
            OnCharacterHealed?.Invoke();
        }

        public void PartialReviveCharacter(float _percentHeal)
        {
            currentHealthPoints += Mathf.RoundToInt(maxHealthPoints * _percentHeal);
            OnCharacterHealed?.Invoke();
        }

        #endregion


    }
}