using Cysharp.Threading.Tasks;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Character.Creations.CreationDatas;
using Runtime.GameControllers;
using UnityEngine;

namespace Runtime.Abilities
{
    public class CreationAbilityEntity: AbilityEntityBase
    {

        #region Serialized Fields

        [SerializeField] private CreationData m_creationData;

        #endregion

        #region Ability Inherited Methods

        public override void OnAbilityUsed() { }

        protected override UniTask PerformAbilityAction()
        {
            currentOwner.TryGetComponent(out CharacterBase _character);
            var direction = targetPosition.FlattenVector3Y() - currentOwner.transform.position.FlattenVector3Y();
            
            CreationController.Instance.GetCreationAt(m_creationData, targetPosition, direction,
                currentOwner.transform, _character.side);
            
            return base.PerformAbilityAction();
        }

        #endregion
        
        
       
    }
}