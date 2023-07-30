using Project.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class AbilityParamTabItem: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private Image background;

        [SerializeField] private TMP_Text tabText;

        #endregion

        #region Class Implementation

        public void InitializeItem(Color _backgroundColor, string _displayText)
        {
            if (!_backgroundColor.IsNull())
            {
                background.color = _backgroundColor;
            }

            if (!string.IsNullOrEmpty(_displayText))
            {
                tabText.text = _displayText;
            }
            
        }

        #endregion

    }
}