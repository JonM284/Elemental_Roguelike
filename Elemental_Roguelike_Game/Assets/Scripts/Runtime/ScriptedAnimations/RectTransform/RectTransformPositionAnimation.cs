using UnityEngine;

namespace Runtime.ScriptedAnimations.RectTransform
{
    public class RectTransformPositionAnimation: RectTransformAnimation
    {
        
        #region Serialized Fields

        [SerializeField] private Vector2 m_from;
        
        [SerializeField] private Vector2 m_to;
        
        #endregion
        
        public override void SetAnimationValue(float progress)
        {
            target.anchoredPosition = Vector3.LerpUnclamped(m_from, m_to, progress);
        }
    }
}