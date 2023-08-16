using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Character.Creations
{
    public class PassiveCreation: CreationBase
    {

        #region CreationBase Inherited Methods

        public override void DoMovementAction()
        {
            //nothing 
        }

        public override void DoAction()
        {
            //nothing
        }

        public override void DestroyCreation()
        {
            localDestroyVFX.PlayAt(transform.position, Quaternion.identity);
            //Cache Creation
            CreationController.Instance.ReturnToPool(this);
        }

        #endregion
        
    }
}