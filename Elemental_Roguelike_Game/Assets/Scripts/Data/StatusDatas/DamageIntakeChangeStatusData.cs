using UnityEngine;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Damage Intake Change Status Data")]
    public class DamageIntakeChangeStatusData: StatusData
    {
        [Header("Status Specific")] 
        public float damageIntakeModifier = 1;
    }
}