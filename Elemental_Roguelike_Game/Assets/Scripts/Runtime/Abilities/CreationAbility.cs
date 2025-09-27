using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Character.Creations.CreationDatas;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Abilities
{
    [CreateAssetMenu(menuName = "Ability/Creation Ability")]
    public class CreationAbility: Ability
    {

        #region Serialized Fields

        [SerializeField] private CreationData m_creationData;

        #endregion

        #region Ability Inherited Methods

        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
                return;
            }
            
            m_targetPosition = _inputPosition.FlattenVector3Y();
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            
        } 
        
        public override void UseAbility(Vector3 _ownerUsePos)
        {
            if (currentOwner.IsNull())
            {
                return;
            }

            currentOwner.TryGetComponent(out CharacterBase _character);
            var direction = m_targetPosition.FlattenVector3Y() - currentOwner.transform.position.FlattenVector3Y();
            
            CreationController.Instance.GetCreationAt(m_creationData, m_targetPosition, direction,
                currentOwner.transform, _character.side);
            base.UseAbility(_ownerUsePos);
        }

        #endregion
        
        
       
    }
}