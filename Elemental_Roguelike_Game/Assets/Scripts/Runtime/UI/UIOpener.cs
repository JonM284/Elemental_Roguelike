using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Runtime.UI
{
    public class UIOpener: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private AssetReferenceT<UIBase> uiWindowAssetRef;

        #endregion
        
        #region Class Impelmentation

        public void OpenUIWindow()
        {
            UIUtils.OpenUI(uiWindowAssetRef, uiWindowAssetRef.editorAsset.uiLayerData);
        }

        #endregion
        
    }
}