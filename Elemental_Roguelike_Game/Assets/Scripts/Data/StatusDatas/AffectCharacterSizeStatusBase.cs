using UnityEngine;

namespace Data.StatusDatas
{
    [CreateAssetMenu(menuName = "Maulball/Status/Change Size Status Data")]
    public class AffectCharacterSizeStatusBase: StatusData
    {
        [Range(0.1f,1.0f)]
        public float m_increasePercentage;
        
        
    }
}