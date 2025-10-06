using UnityEngine;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Lifesteal Status Data")]
    public class LifestealStatusData: StatusData
    {
        [Header("Lifesteal Specific")] 
        [Range(0.01f, 1f)] public float lifeStealPercentage;
    }
}