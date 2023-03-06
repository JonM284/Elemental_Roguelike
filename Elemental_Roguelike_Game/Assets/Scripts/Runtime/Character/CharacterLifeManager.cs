using System;
using Data;
using Data.Elements;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Events

        public static event Action<CharacterBase> OnCharacterDied;

        public static event Action OnCharacterHealed;
        

        #endregion

        #region Private Fields

        private CharacterBase m_ownCharacter;
        
        #endregion

        #region Accessors
        
        public int currentHealthPoints { get; private set; }

        public int currentShieldPoints { get; private set; } 

        public int maxHealthPoints { get; private set; }

        public int maxSheildPoints { get; private set; }
        
        public ElementTyping characterElementType { get; private set; }

        public bool isAlive => currentHealthPoints > 0;

        public CharacterBase ownCharacter => CommonUtils.GetRequiredComponent(ref m_ownCharacter, () =>
        {
            var cb = GetComponent<CharacterBase>();
            return cb;
        });

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
            }
            else
            {
                if (_incomingDamage >= currentShieldPoints)
                {
                    currentShieldPoints = 0;
                    currentHealthPoints -= (_fixedIncomingDamage - currentShieldPoints);
                }
                else
                {
                    currentShieldPoints -= _fixedIncomingDamage;
                }
            }
            
            Debug.Log($"<color=orange> {this.gameObject.name} took damage /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");

            if (currentHealthPoints <= 0)
            {
                OnCharacterDied?.Invoke(ownCharacter);
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