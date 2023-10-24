using System;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Events

        public static event Action<CharacterBase> OnCharacterDied;

        public static event Action<CharacterBase> OnCharacterHealthChange;
        
        public static event Action<CharacterBase,int> CharacterTookDamage;
        

        #endregion

        #region Serialized Fields

        [SerializeField] private Transform healthBarPos;

        #endregion

        #region Private Fields

        private CharacterBase m_ownCharacter;

        private float m_damageMod = 1f;

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

        public void SetDamageIntakeModifier(float _newModifierAmount)
        {
            m_damageMod = _newModifierAmount;
        }

        public void DealDamage(Transform _attacker, int _incomingDamage, bool _armorPiercing, ElementTyping _type)
        {
            var _fixedIncomingDamage = Mathf.RoundToInt(_incomingDamage * m_damageMod);
            
            if (_armorPiercing)
            {
                currentHealthPoints -= _fixedIncomingDamage;
            }
            else
            {
                if (_fixedIncomingDamage >= currentShieldPoints)
                {
                    currentHealthPoints -= (_fixedIncomingDamage - currentShieldPoints);
                    currentShieldPoints = 0;
                }
                else
                {
                    currentShieldPoints -= _fixedIncomingDamage;
                }
            }
            
            OnCharacterHealthChange?.Invoke(ownCharacter);
            
            JuiceController.Instance.CreateDamageText(_fixedIncomingDamage, healthBarPos.position);

            if (!_attacker.IsNull())
            {
                _attacker.TryGetComponent(out CharacterBase _character);
                if (_character)
                {
                    CharacterTookDamage?.Invoke(_character, _fixedIncomingDamage);
                }    
            }

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
            OnCharacterHealthChange?.Invoke(ownCharacter);
        }

        public void PartialReviveCharacter(float _percentHeal)
        {
            currentHealthPoints += Mathf.RoundToInt(maxHealthPoints * _percentHeal);
            OnCharacterHealthChange?.Invoke(ownCharacter);
        }

        public void HealCharacter(int _healAmount, bool _isHealArmor = false)
        {
            var adjustedAmount = Mathf.Abs(_healAmount);
            
            if (_isHealArmor)
            {
                if (currentShieldPoints >= maxSheildPoints)
                {
                    return;
                }

                if (currentShieldPoints + adjustedAmount > maxSheildPoints)
                {
                    currentShieldPoints = maxSheildPoints;
                    return;
                }
                
                Debug.Log($"<color=green> {this.gameObject.name} Health{!_isHealArmor}:Shield{_isHealArmor} // " +
                          $"Healed for Amount:{adjustedAmount} /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");
                currentShieldPoints = adjustedAmount;
                return;
            }

            if (currentHealthPoints >= maxHealthPoints)
            {
                return;
            }
            
            if (currentHealthPoints + adjustedAmount > maxHealthPoints)
            {
                currentHealthPoints = maxHealthPoints;
                return;
            }
            
            currentHealthPoints += adjustedAmount;
            
            OnCharacterHealthChange?.Invoke(ownCharacter);
            
            Debug.Log($"<color=green> {this.gameObject.name} Health{!_isHealArmor}:Shield{_isHealArmor} // " +
                      $"Healed for Amount:{adjustedAmount} /// hp now: {currentHealthPoints} shields: {currentShieldPoints} </color>");

        }


        [ContextMenu("InstaKill")]
        public void InstaKill()
        {
            DealDamage(null, 1000, false,null);
        }
        #endregion


    }
}