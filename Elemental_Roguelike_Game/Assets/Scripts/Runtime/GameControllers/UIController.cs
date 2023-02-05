using System.Collections.Generic;
using Project.Scripts.Utils;
using Runtime.UI;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class UIController: GameControllerBase
    {
        #region Serialize Fields

        [SerializeField] private Canvas popupCanvas;

        [SerializeField] private Canvas mainCanvasOverlay;

        [SerializeField] private Canvas mainCanvas;

        [SerializeField] private Canvas mainCanvasBackground;

        #endregion

        #region Private Fields

        private List<UIBase> m_cachedUIWindows = new List<UIBase>();

        private Transform m_cachedUIPoolTransform;
        
        #endregion

        #region Accessors

        public Transform cachedUIPool =>
            CommonUtils.GetRequiredComponent(ref m_cachedUIPoolTransform, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });

        #endregion

        #region Class Impelmentation

        public void AddPopup()
        {
            
        }

        public void ReturnUIToCachedPool(UIBase _uiWindow)
        {
            if (_uiWindow == null)
            {
                return;
            }
            
            m_cachedUIWindows.Add(_uiWindow);
            _uiWindow.uiRectTransform.ResetTransform(cachedUIPool);
        }

        #endregion
        
    }
}