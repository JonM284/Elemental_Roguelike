using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UIBase: MonoBehaviour
    {

        #region Private Fields

        private RectTransform m_uiRectTransform;

        #endregion

        #region Accessors

        public RectTransform uiRectTransform => CommonUtils.GetRequiredComponent(ref m_uiRectTransform, () =>
        {
            var rt = GetComponent<RectTransform>();
            return rt;
        });

        #endregion

        #region Class Implementation

        private void Close()
        {
            UIUtils.CloseUI(this);
        }

        #endregion
        
    }
}