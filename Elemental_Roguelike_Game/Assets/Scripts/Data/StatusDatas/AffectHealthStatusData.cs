using UnityEngine;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Health Change Status Data")]
    public class AffectHealthStatusData: StatusData
    {
        [Header("Status Specific")]
        public bool isOverTime, isOnApply, isArmorPiercing;
        public int amountChange;
    }
}