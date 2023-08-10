using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Marked Status")]
    public class MarkedStatus: Status
    {

        #region Serialized Fields

        [SerializeField] private float damageModAmount = 1.5f;
        
        #endregion
        
        #region Private Fields

        private float m_normalAmount = 1f;

        #endregion

        #region Status Inherited Methods

        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.characterLifeManager.SetDamageIntakeModifier(damageModAmount);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.characterLifeManager.SetDamageIntakeModifier(m_normalAmount);
        }
        

        #endregion
        
    }
}