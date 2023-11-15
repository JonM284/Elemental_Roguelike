using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    public abstract class GameplayItemData: ScriptableObject
    {
        public string itemName;
        public AssetReference itemImage;

        //Do Something
        public abstract void InitializeAffects(CharacterBase _character);
    }
}