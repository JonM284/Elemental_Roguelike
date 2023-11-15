using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Freeze Status")]
    public class FreezeStatus: Status
    {
        
        #region Serialized Fields

        [SerializeField] private float damageModAmount = 1.25f;
        
        #endregion
        
        #region Private Fields

        private float m_normalAmount = 1f;

        #endregion
        
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            _character.SetCharacterUsable(false);
            _character.characterClassManager.SetAbleToReact(false);

            if (!_character.heldBall.IsNull())
            {
                _character.KnockBallAway();
            }
            
            _character.characterLifeManager.SetDamageIntakeModifier(damageModAmount);

        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.SetCharacterUsable(true);
            _character.characterClassManager.SetAbleToReact(true);
            _character.characterLifeManager.SetDamageIntakeModifier(m_normalAmount);
        }
        
        
    }
}