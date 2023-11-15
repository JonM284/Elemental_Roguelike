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

        public override void InitializeAffects(CharacterBase _character)
        {
            
        }

        #endregion
        
        
    }
}