﻿using UnityEngine;

namespace Runtime.ScriptedAnimations.Transform
{
    public class TransformPositionAnimation : TransformAnimation
    {

        #region Serialized Fields

        [SerializeField] private Vector3 m_from;
        
        [SerializeField] private Vector3 m_to;
        
        #endregion
        
        public override void SetAnimationValue(float progress)
        {
            target.localPosition = Vector3.LerpUnclamped(m_from, m_to, progress);
        }
    }
}