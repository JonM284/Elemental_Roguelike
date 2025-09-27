using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character.StateMachines
{
    public class OverwatchState: StateBase
    {
     
        #region Private Fields

        private CharacterClassManager classManagerRef;

        #endregion
        
        #region Accessors

        public CharacterClassManager classManager => CommonUtils.GetRequiredComponent(ref classManagerRef,
            GetComponentInParent<CharacterClassManager>);

        #endregion
        
        #region StateBase Inherited Methods

        public override void EnterState()
        {
            classManager.ActivateCharacterOverwatch();
        }

        public override void AssignArgument(params object[] _arguments)
        {
            
        }
        
        public override void UpdateState()
        {
            classManager.OverwatchUpdate();
        }

        public override void MarkHighlight(Vector3 _position)
        {
            
        }

        public override void SelectTarget(Vector3 _position)
        {
            
        }

        public override void ExitState()
        {
            
        }

        #endregion
        
    }
}