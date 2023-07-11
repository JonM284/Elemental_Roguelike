using Data.Elements;
using Runtime.Damage;
using UnityEngine;

namespace Runtime.Environment
{
    public class CoverObstacles: MonoBehaviour, IDamageable
    {

        #region Serialized Fields

        [SerializeField] private ObstacleType obstacleType;

        [SerializeField] private int amountOfHitsMax;

        #endregion

        #region Private Fields

        private int m_currentAmountOfHits;
        private IDamageable damageableImplementation;

        #endregion

        #region Accessors

        public ObstacleType type => obstacleType;

        #endregion

        #region Class Implementation

        public void InitializeCover()
        {
            m_currentAmountOfHits = amountOfHitsMax;
        }

        #endregion

        #region IDamageable Inherited Methods

        public void OnRevive()
        {
            //Nothing for now
        }

        public void OnDealDamage(Transform _attacker, int _damageAmount, bool _armorPiercing, ElementTyping _damageElementType, bool _hasKnockback)
        {
            m_currentAmountOfHits--;
            if (m_currentAmountOfHits <= 0)
            {
                //ToDo: Destroy obstacle
            }
        }

        #endregion
    }
}