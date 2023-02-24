using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    [Serializable] 
    [CreateAssetMenu(menuName = "Custom Data/Window Dialog Data")]
    public class UIWindowData: ScriptableObject
    {
        public UILayerData layerData;
        
        public AssetReference uiWindowAssetReference;
    }
}