using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.ScriptedAnimations.RectTransform
{
    public class ColorAnimations: RectTransformAnimation
    {
        
        #region Serialized Fields

        [SerializeField] private Color m_from;
        
        [SerializeField] private Color m_to;
        
        #endregion

        #region Private Fields

        private Graphic m_targetImage;

        #endregion

        #region Accessors

        public Graphic targetImage => CommonUtils.GetRequiredComponent(ref m_targetImage, () =>
        {
            var i = target.GetComponent<Graphic>();
            return i;
        });

        #endregion
        
        public override void SetAnimationValue(float progress)
        {
            targetImage.color = Color.LerpUnclamped(m_from, m_to, progress);
        }
        
        
    }
}