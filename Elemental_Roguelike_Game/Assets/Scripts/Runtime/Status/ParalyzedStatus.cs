using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;

namespace Runtime.Status
{
    [CreateAssetMenu(menuName = "Status/Paralyze Status")]
    public class ParalyzedStatus: Status
    {

        #region Serialized Fields

        [Range(10,100)]
        [SerializeField] private int chanceToParalyze = 10;

        #endregion
        
        public override void TriggerStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var randomAmount = Random.Range(0,100);
            if (randomAmount >= chanceToParalyze)
            {
                return;
            }
            
            _character.SetCharacterUsable(false);
        }

        public override void ResetStatusEffect(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            _character.SetCharacterUsable(false);
        }
    }
}