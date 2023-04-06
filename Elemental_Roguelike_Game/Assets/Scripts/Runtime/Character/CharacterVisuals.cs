using System.Collections.Generic;
using Data.Elements;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterVisuals : MonoBehaviour
    {

        #region Read-Only

        private static readonly int LightColor = Shader.PropertyToID("_LightColor");
        
        private static readonly int DarkColor = Shader.PropertyToID("_DarkColor");
        
        #endregion
        
        #region Serialized Fields

        [SerializeField] private GameObject characterVisual;

        [SerializeField] private List<SkinnedMeshRenderer> meepleSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        [SerializeField] private List<MeshRenderer> meepleMeshRenderers = new List<MeshRenderer>();

        #endregion

        #region Private Fields

        private Material m_clonedMaterial;

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

        public void InitializeMeepleCharacterVisuals(ElementTyping _type)
        {
            if (meepleSkinnedMeshRenderers.Count == 0)
            {
                Debug.LogError("No Character Visuals");
                return;
            }

            m_clonedMaterial = new Material(meepleSkinnedMeshRenderers[0].material);

            meepleSkinnedMeshRenderers.ForEach(smr =>
            {
                smr.material = m_clonedMaterial;
            });
            
            meepleMeshRenderers.ForEach(mr =>
            {
                mr.material = m_clonedMaterial;
            });
            
            m_clonedMaterial.SetColor(LightColor, _type.meepleColors[0]);
            m_clonedMaterial.SetColor(DarkColor, _type.meepleColors[1]);
            
        }

        #endregion

    }
}