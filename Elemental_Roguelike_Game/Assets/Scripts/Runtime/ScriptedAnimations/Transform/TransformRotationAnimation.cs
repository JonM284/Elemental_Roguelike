using UnityEngine;

namespace Runtime.ScriptedAnimations.Transform
{
    public class TransformRotationAnimation : TransformAnimation
    {

        #region Serialized Fields

        #region Serialized Fields

        [SerializeField] private Vector3 m_from;
        
        [SerializeField] private Vector3 m_to;
        
        #endregion
        
        #endregion
        
        public override void SetAnimationValue(float progress)
        {
            target.localRotation = Quaternion.Euler(Vector3.LerpUnclamped(m_from, m_to, progress));
        }
    }
}