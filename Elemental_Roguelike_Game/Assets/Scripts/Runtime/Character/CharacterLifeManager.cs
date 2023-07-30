using System;
using Data.Elements;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Events

        public static event Action<CharacterBase> OnCharacterDied;

        public static event Action OnCharacterHealthChange;
        

        #endregion

        #region Serialized Fields

        [SerializeField] private Transform healthBarPos;

        #endregion

        #region Private Fields

        private CharacterBase m_ownCharacter;
        
        #endregion

        #region Accessors
        
        public int currentHealthPoints { get; private set; }

        public int currentShieldPoints { get; private set; }

        public int currentOverallHealth => currentHealthPoints + currentShieldPoints;

        public int maxHealthPoints { get; private set; }

        public int maxSheildPoints { get; private set; }
        
        public ElementTyping characterElementType { get; private set; }

        public bool isAlive => currentHealthPoints > 0;

        public CharacterBase ownCharacter => CommonUtils.GetRequiredComponent(ref m_ownCharacter, () =>
        {
            var cb = GetComponent<CharacterBase>();
            return cb;
        });

        public Transform healthBarFollowPos => healthBarPos;
        
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
            if (characterElementType == null)
            {
                Debug.LogError("<color=cyan>NULL is characterElementType</color>");
                return;
            }

            var _fixedIncomingDamage = _incomingDamage;
            
            if (_type != null && _incomingDamage > 0)
            {
                _fixedIncomingDamage = characterElementType.CalculateDamageOnWeakness(_incomingDamage, _type);
            }
            
            if (_armorPiercing)
            {
                currentHealthPoints -= _fixedIncomingDamage;
            }
            else
            {
                if (_incomingDamage >= currentShieldPoints)
                {
                    currentHealthPoints -= (_fixedIncomingDamage - currentShieldPoints);
                    currentShieldPoints = 0;
                }
                else
                {
                    currentShieldPoints -= _fixedIncomingDamage;
                }
            }
            
            OnCharacterHealthChange?.Invoke();


            Debug.Log($"<color=orange> {this.gameObject.name} took damage Amount:{_incomingDamage} /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");

            if (currentHealthPoints <= 0)
            {
                OnCharacterDied?.Invoke(ownCharacter);
            }
        }

        public void FullReviveCharacter()
        {
            currentHealthPoints = maxHealthPoints;
            currentShieldPoints = maxSheildPoints;
            OnCharacterHealthChange?.Invoke();
        }

        public void PartialReviveCharacter(float _percentHeal)
        {
            currentHealthPoints += Mathf.RoundToInt(maxHealthPoints * _percentHeal);
            OnCharacterHealthChange?.Invoke();
        }

        public void HealCharacter(int _healAmount, bool _isHealArmor = false)
        {
            if (_isHealArmor)
            {
                if (currentShieldPoints >= maxSheildPoints)
                {
                    return;
                }

                if (currentShieldPoints + _healAmount > maxSheildPoints)
                {
                    currentShieldPoints = maxSheildPoints;
                    return;
                }
                
                Debug.Log($"<color=green> {this.gameObject.name} Health{!_isHealArmor}:Shield{_isHealArmor} // " +
                          $"Healed for Amount:{_healAmount} /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");
                currentShieldPoints += _healAmount;
                return;
            }

            if (currentHealthPoints >= maxHealthPoints)
            {
                return;
            }
            
            if (currentHealthPoints + _healAmount > maxHealthPoints)
            {
                currentHealthPoints = maxHealthPoints;
                return;
            }
            
            currentHealthPoints += _healAmount;
            
            OnCharacterHealthChange?.Invoke();
            
            Debug.Log($"<color=green> {this.gameObject.name} Health{!_isHealArmor}:Shield{_isHealArmor} // " +
                      $"Healed for Amount:{_healAmount} /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");

        }

        #endregion


    }
}