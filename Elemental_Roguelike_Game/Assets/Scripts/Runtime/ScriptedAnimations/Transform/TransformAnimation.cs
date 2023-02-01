using UnityEngine;

namespace Runtime.ScriptedAnimations.Transform
{
    public abstract class TransformAnimation : TransformAnimationsBase
    {
        #region Serialized Fields

        [SerializeField] private UnityEngine.Transform _target;

        #endregion
        
        #region Accessors

        protected UnityEngine.Transform target => _target;

        #endregion
    }
}