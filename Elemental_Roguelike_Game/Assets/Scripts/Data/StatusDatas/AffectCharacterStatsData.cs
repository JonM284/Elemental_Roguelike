using Runtime.Character;
using UnityEngine;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Change Stats Status Data")]
    public class AffectCharacterStatsData: StatusData
    {

        [Header("Affect Character Stats Specific")]
        public CharacterStatsEnum targetStat = CharacterStatsEnum.AGILITY;

        [Tooltip("Can be + or -, + = BUFF, - = DEBUFF")]
        public int amountToChangeBy; 
        
        [Tooltip("Does the stat continuously go up or down while the status is active?")]
        public bool isChangeOverTime;
        
    }
}