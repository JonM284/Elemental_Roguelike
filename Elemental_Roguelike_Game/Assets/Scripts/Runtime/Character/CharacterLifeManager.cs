using Data;
using Data.Elements;
using Project.Scripts.Data;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Serialized Fields


        #endregion

        #region Private Fields

        private float m_currentHealthPoints;

        private float m_currentShieldPoints;

        private CharacterStatsData m_characterStats;

        #endregion

        #region Accessors

        public float maxHealthPoints => m_characterStats.baseHealth;

        public float maxSheildPoints => m_characterStats.baseShields;

        public bool isAlive => m_currentHealthPoints > 0;

        #endregion

        #region Class Implementation

        public void InitializeCharacterHealth(CharacterStatsData _stats)
        {
            m_characterStats = _stats;
        }

        public void DealDamage(float _incomingDamage, bool _armorPiercing, ElementTyping _type)
        {
            var _fixedIncomingDamage = m_characterStats.type.CalculateDamageOnWeakness(_incomingDamage, _type);
            
            if (_armorPiercing)
            {
                m_currentHealthPoints -= _fixedIncomingDamage;
                return;
            }

            if (_incomingDamage >= m_currentShieldPoints)
            {
                m_currentShieldPoints = 0;
                m_currentHealthPoints -= (_fixedIncomingDamage - m_currentShieldPoints);
            }
            else
            {
                m_currentShieldPoints -= _fixedIncomingDamage;
            }
        }

        public void FullReviveCharacter()
        {
            m_currentHealthPoints = maxHealthPoints;
            m_currentShieldPoints = maxSheildPoints;
        }

        public void PartialReviveCharacter(float _percentHeal)
        {
            m_currentHealthPoints += maxHealthPoints * _percentHeal;
        }

        #endregion


    }
}