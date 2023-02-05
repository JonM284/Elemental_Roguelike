using Project.Scripts.Data;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private CharacterStatsBase characterStats;

        #endregion

        #region Private Fields

        private float m_currentHealthPoints;

        private float m_currentShieldPoints;

        #endregion

        #region Accessors

        public float maxHealthPoints => characterStats.baseHealth;

        public float maxSheildPoints => characterStats.baseShields;

        public bool isAlive => m_currentHealthPoints > 0;

        #endregion

        #region Class Implementation

        public void DealDamage(float _incomingDamage, bool _armorPiercing)
        {
            if (_armorPiercing)
            {
                m_currentHealthPoints -= _incomingDamage;
                return;
            }

            if (_incomingDamage >= m_currentShieldPoints)
            {
                m_currentShieldPoints = 0;
                m_currentHealthPoints -= (_incomingDamage - m_currentShieldPoints);
            }
            else
            {
                m_currentShieldPoints -= _incomingDamage;
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