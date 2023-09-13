using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(menuName = "Item/Stat Upgrade")]
    public class StatsUpgradeItem: GameplayItemData
    {

        [SerializeField] private CharacterStatsEnum statType;

        [Header("Percentage")]
        
        [SerializeField] private bool isPercentage;
        
        [Range(0,1)]
        [SerializeField] private float percentChange;
        
        [Header("Exact Amount")]
        [SerializeField] private int amountChange;

        #region GameplayItemData Inherited Methods

        public override void DoEffects(CharacterBase _character)
        {
            
        }

        public override float GetAffectedFloat(float _numToModify)
        {
            var _amountChange = isPercentage ? _numToModify * percentChange : amountChange;
            var _returnAmount = _numToModify + _amountChange;
            return _returnAmount;
        }

        public override int GetAffectFloat(int _numToModify)
        {
            return _numToModify + amountChange;
        }

        #endregion
        
        
    }
}