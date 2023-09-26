using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Fortify Status")]
    public class FortifyStatus: Status
    {

        #region Serialized Fields

        [SerializeField] private float fortifyDamageAmount = 0.5f;

        [SerializeField] private float fortifyKnockbackReductionMod = 0.5f;
        
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
            
            _character.characterMovement.SetKnockbackable(fortifyKnockbackReductionMod);
            _character.characterLifeManager.SetDamageIntakeModifier(fortifyDamageAmount);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.characterMovement.SetKnockbackable(m_normalAmount);
            _character.characterLifeManager.SetDamageIntakeModifier(m_normalAmount);
        }
        
        #endregion
    }
}