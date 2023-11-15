using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    
    [CreateAssetMenu(menuName = "Status/Shrink Status")]
    public class ShrinkStatus: Status
    {
        #region Public Fields

        [Range(0.1f,1.0f)]
        public float m_decreasePercentage;

        #endregion

        #region Class Implementation

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.transform.localScale = Vector3.one * m_decreasePercentage;
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.transform.localScale = Vector3.one;
        }

        #endregion
    }
}