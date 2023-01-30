using Project.Scripts.Data;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterLifeManager
    {

        #region Serialized Fields

        [SerializeField] private CharacterStatsBase characterStats;

        #endregion

        #region Accessors

        public float healthPoints => characterStats.baseHealth;

        public float sheildPoints => characterStats.baseShields;

        #endregion


    }
}