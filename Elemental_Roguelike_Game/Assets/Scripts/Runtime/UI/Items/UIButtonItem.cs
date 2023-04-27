using System;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.Items
{
    public class UIButtonItem : MonoBehaviour
    {

        #region Private Fields

        private Button m_button;

        #endregion

        #region Accessor

        private Button button => CommonUtils.GetRequiredComponent(ref m_button, () =>
        {
            var b = GetComponent<Button>();
            return b;
        });

        #endregion

        #region Class Implementation

        public void InitializeItem(Action callBack)
        {
            if (callBack == null)
            {
                return;
            }
            
            button.onClick.AddListener(() =>
            {
                callBack?.Invoke();
                Debug.Log("Pressed");
            });
        }

        #endregion
        
        
    }
}