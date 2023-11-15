using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Grow Status")]
    public class GrowStatus: Status
    {

        #region Public Fields

        [Range(0.1f,1.0f)]
        public float m_increasePercentage;

        #endregion

        #region Class Implementation

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var amountIncrease = Vector3.one * m_increasePercentage;

            _character.transform.localScale = Vector3.one + amountIncrease;
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