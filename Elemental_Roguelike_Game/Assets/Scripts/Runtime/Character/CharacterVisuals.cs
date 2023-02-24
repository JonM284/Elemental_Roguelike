using UnityEngine;

namespace Runtime.Character
{
    public class CharacterVisuals : MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private GameObject characterVisual;

        #endregion

        #region Accessors

        public GameObject characterModel { get; private set; }

        #endregion

        #region Class Implementation

        public void InitializeCharacterVisuals(GameObject _newCharacterModel)
        {
            if (characterVisual != null)
            {
                characterModel = characterVisual;
                return;
            }

            characterVisual = _newCharacterModel;
            characterModel = _newCharacterModel;
        }

        #endregion

    }
}