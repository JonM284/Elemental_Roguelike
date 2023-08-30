using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.ScriptedAnimations;
using Runtime.ScriptedAnimations.RectTransform;
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

        #endregion

        #region Class Implementation

        public void Initialize(int _damageAmount)
        {
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