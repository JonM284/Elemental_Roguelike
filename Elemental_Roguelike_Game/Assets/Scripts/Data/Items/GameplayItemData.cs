using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Data
{
    public abstract class GameplayItemData: ScriptableObject
    {
        public string itemName;
        public AssetReference itemImage;

        public abstract void DoEffects(CharacterBase _character);

        public abstract float GetAffectedFloat(float _numToModify);

        public abstract int GetAffectFloat(int _numToModify);
    }
}