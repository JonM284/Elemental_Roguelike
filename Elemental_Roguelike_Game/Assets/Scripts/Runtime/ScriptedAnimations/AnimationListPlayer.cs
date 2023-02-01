using System.Collections.Generic;
using Runtime.ScriptedAnimations.Transform;
using UnityEngine;

namespace Runtime.ScriptedAnimations
{
    public class AnimationListPlayer : TransformAnimationsBase
    {

        #region Serialized Fields

        [SerializeField] private List<TransformAnimationsBase> m_animations = new List<TransformAnimationsBase>();

        #endregion
        public override void SetProgress(float progress)
        {
            m_animations.ForEach(a => a.SetProgress(progress));
        }

        public override void SetAnimationValue(float progress)
        {
            
        }
    }
}