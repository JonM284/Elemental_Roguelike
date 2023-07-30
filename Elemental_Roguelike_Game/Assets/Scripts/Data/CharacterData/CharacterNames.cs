using System.Collections.Generic;
using UnityEngine;

namespace Data.CharacterData
{
    [CreateAssetMenu(menuName = "Custom Data/Character Random Names")]
    public class CharacterNames: ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private List<string> possibleCharacterNames = new List<string>();

        #endregion

        #region Accessors

        public List<string> randomCharacterNames => possibleCharacterNames;

        #endregion

    }
}