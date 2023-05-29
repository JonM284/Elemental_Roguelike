using System;
using Data;
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

        public void InitializeSelectionItem(CharacterStatsData meeple, Action<CharacterStatsData> callBack)
        {
            Debug.Log("Initializing Button");
            if (meeple.IsNull() || callBack == null)
            {
                Debug.Log("MeepleID empty or callback null");
                return;
            }
            
            button.onClick.AddListener(() =>
            {
                callBack?.Invoke(meeple);
                Debug.Log("Pressed");
            });
        }

        #endregion
        
        
    }
}