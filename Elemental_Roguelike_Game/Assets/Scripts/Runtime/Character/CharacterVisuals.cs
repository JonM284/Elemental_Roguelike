using System.Collections.Generic;
using Data.Elements;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterVisuals : MonoBehaviour
    {

        #region Read-Only

        private static readonly int LightColor = Shader.PropertyToID("_LightColor");
        
        private static readonly int DarkColor = Shader.PropertyToID("_DarkColor");
        
        private static readonly int OutlineThickness = Shader.PropertyToID("_OutlineThickness");
        
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

        #endregion
        
        #region Serialized Fields

        [SerializeField] private GameObject characterVisual;

        [SerializeField] private Color highlightColor;

        [SerializeField] private bool m_isMeeple;

        [SerializeField] private List<SkinnedMeshRenderer> meepleSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

        [SerializeField] private List<MeshRenderer> meepleMeshRenderers = new List<MeshRenderer>();

        #endregion

        #region Private Fields

        private Material m_clonedMaterial;

        private float m_originalHighlightThickness;

        private float m_highlightMaxThickness = 0.05f;

        private Color m_originalHighlightColor;

        private Material m_highlightMaterial;

        #endregion

        #region Accessors

        public GameObject characterModel { get; private set; }

        #endregion

        #region Class Implementation

        public void InitializeCharacterVisuals()
        {
            if (characterVisual != null)
            {
                characterModel = characterVisual;
            }

            var _mat = characterModel.GetComponent<SkinnedMeshRenderer>().materials[0];
            if (_mat.IsNull())
            {
                Debug.Log("Can not find material // or skinned mesh renderer");
                return;
            }

            InitializeHighlightVariables(_mat);


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

            InitializeHighlightVariables(m_clonedMaterial);
        }

        private void InitializeHighlightVariables(Material _associatedMaterial)
        {
            m_highlightMaterial = _associatedMaterial;

            m_originalHighlightThickness = m_highlightMaterial.GetFloat(OutlineThickness);
            m_originalHighlightColor = m_highlightMaterial.GetColor(OutlineColor);
        }
        
        public void SetHighlight()
        {
            m_highlightMaterial.SetFloat(OutlineThickness, m_highlightMaxThickness);
            m_highlightMaterial.SetColor(OutlineColor, highlightColor);
        }

        public void SetUnHighlight()
        {
            m_highlightMaterial.SetFloat(OutlineThickness, m_originalHighlightThickness);
            m_highlightMaterial.SetColor(OutlineColor, m_originalHighlightColor);
        }

        public void SetNewLayer(LayerMask _preferredLayer)
        {
            if (m_isMeeple)
            {
                meepleSkinnedMeshRenderers.ForEach(smr => smr.gameObject.layer = _preferredLayer);
                meepleMeshRenderers.ForEach(mr => mr.gameObject.layer = _preferredLayer);     
            }
            else
            {
                characterVisual.layer = _preferredLayer;
            }
            
            Debug.Log($"Changed to {_preferredLayer.value}");
        }


        //ToDo: Implement Feature
        /// <summary>
        /// This will randomize facial colors, images, clothing, etc
        /// </summary>
        public void RandomizeMeepleDecorations()
        {
            
        }

        #endregion

    }
}