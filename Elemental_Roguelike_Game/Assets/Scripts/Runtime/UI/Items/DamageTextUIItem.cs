using Runtime.GameControllers;
using Runtime.ScriptedAnimations;
using Runtime.ScriptedAnimations.Transform;
using TMPro;
using UnityEngine;

namespace Runtime.UI.Items
{
    public class DamageTextUIItem: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private RelativeTransformPositionAnimation textAnimation;

        [SerializeField] private AnimationsBase fadeAnimation;

        [SerializeField] private TMP_Text damageText;

        [SerializeField] private int criticalDamageThreshold = 20;

        [SerializeField] private Color criticalColor;

        [SerializeField] private Color healingColor;

        #endregion

        #region Class Implementation

        public void Initialize(int _damageAmount)
        {
            if (_damageAmount < 0)
            {
                damageText.color = healingColor;
            }else if (_damageAmount > criticalDamageThreshold)
            {
                damageText.color = criticalColor;
            }
            else
            {
                damageText.color = Color.white;
            }
            
            damageText.text = $"{_damageAmount}";
            textAnimation.Initialize();
            fadeAnimation.Play();
        }

        public void Close()
        {
            JuiceController.Instance.CacheDamageText(this);
        }

        #endregion

    }
}