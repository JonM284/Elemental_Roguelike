using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.GameControllers
{
    public class UIController: GameControllerBase
    {
        #region Nested Classes

        [Serializable]
        public class CanvasByLayer
        {
            public UILayerData layer;
            public Canvas associatedCanvas;
        }
        
        [Serializable]
        public class ModalsByLayer
        {
            public UILayerData layer;
            public AssetReference modalAssetReference;
        }

        #endregion
        
        #region Serialize Fields

        [SerializeField] private ModalsByLayer popupAssetReference;

        [SerializeField] private List<CanvasByLayer> canvasByLayers = new List<CanvasByLayer>();

        #endregion

        #region Private Fields

        private List<UIBase> m_cachedUIWindows = new List<UIBase>();

        [SerializeField] private List<UIPopupDialog> m_cachedPopups = new List<UIPopupDialog>();

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

        public void AddUI(UILayerData _layer, AssetReference _uiWindow)
        {
            if (_layer == null)
            {
                return;
            }

            var _foundCanvasByLayer = canvasByLayers.FirstOrDefault(cbl => cbl.layer == _layer);
            
            //using addressables
            _uiWindow.CloneAddressable(_foundCanvasByLayer.associatedCanvas.transform);
        }

        public void ReturnUIToCachedPool(UIBase _uiWindow)
        {
            if (_uiWindow == null)
            {
                return;
            }

            if (_uiWindow is UIPopupDialog popup)
            {
                m_cachedPopups.Add(popup);
                popup.uiRectTransform.ResetTransform(cachedUIPool);
                return;
            }
            
            m_cachedUIWindows.Add(_uiWindow);
            _uiWindow.uiRectTransform.ResetTransform(cachedUIPool);
        }

        public void CreatePopup(PopupDialogData _data, Action _confirmAction, Action _closeAction = null)
        {
            var foundPopup = GetCachedUIPopup(_data);
            var _layer = popupAssetReference.layer;
            var _foundCanvasByLayer = canvasByLayers.FirstOrDefault(cbl => cbl.layer == _layer);
            
            //If the popup already exists, use same popup
            if (foundPopup != null)
            {
                m_cachedPopups.Remove(foundPopup);
                foundPopup.uiRectTransform.ResetTransform(_foundCanvasByLayer.associatedCanvas.transform);
                Debug.Log("Found Popup");
                return;
            }
            
            //Otherwise create a new popup
            object[] arg = new object[] {_data, _confirmAction, _closeAction};

            var handle = Addressables.LoadAssetAsync<GameObject>(popupAssetReference.modalAssetReference);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newPopupObject = Instantiate(handle.Result, _foundCanvasByLayer.associatedCanvas.transform);
                    var newPopup = newPopupObject.GetComponent<UIPopupDialog>();
                    if (newPopup != null)
                    {
                        newPopup.AssignArguments(arg);
                    }
                }
            };
            
        }

        private UIPopupDialog GetCachedUIPopup(PopupDialogData _dialogData)
        {
            if (_dialogData == null || m_cachedPopups.Count == 0)
            {
                return default;
            }

            var cachedPopup = m_cachedPopups.FirstOrDefault(uip => uip.data == _dialogData);
            return cachedPopup;
        }

        #endregion
        
    }
}