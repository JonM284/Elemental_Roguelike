using System;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class MeepleManagerSelectionItem: MonoBehaviour
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

        public void InitializeSelectionItem(string meepleID, Action<string> callBack)
        {
            Debug.Log("Initializing Button");
            if (meepleID == String.Empty || callBack == null)
            {
                Debug.Log("MeepleID empty or callback null");
                return;
            }
            
            button.onClick.AddListener(() =>
            {
                callBack?.Invoke(meepleID);
                Debug.Log("Pressed");
            });
        }

        #endregion
        
        
    }
}