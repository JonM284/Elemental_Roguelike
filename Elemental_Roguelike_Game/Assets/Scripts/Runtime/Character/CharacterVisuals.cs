using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data.CharacterData;
using Data.Elements;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.ScriptedAnimations.Transform;
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

        [SerializeField] private TransformScaleAnimation m_highlightAnimation;
        
        [SerializeField] private Transform modelParent;
        
        [SerializeField] private Color highlightColor;

        [SerializeField] private bool m_isMeeple;

        [SerializeField] private bool m_isChangeColor;

        [Header("Normal")]
        [SerializeField] private SkinnedMeshRenderer m_skinnedMeshRenderer;
        [SerializeField] private MeshRenderer m_meshRenderer;
        
        
        [Header("Meeple")]
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

        public bool isMeeple => m_isMeeple;

        public Transform ballHandPos { get; private set; }

        #endregion

        #region Class Implementation

        public async UniTask InitializeCharacterVisuals(CharacterStatsBase _data, Transform _ballHoldPos)
        {
            //Instantiate character model and attach to model parent
            if (modelParent.IsNull())
            {
                return;
            }
            
            characterModel = Instantiate(_data.characterModelAssetRef, modelParent.transform.position, modelParent.rotation);
            characterModel.transform.SetParent(modelParent);
            characterModel.transform.localPosition = Vector3.zero;
            
            //Get material attached to mesh
            m_skinnedMeshRenderer = characterModel.GetComponentInChildren<SkinnedMeshRenderer>();
            m_meshRenderer = characterModel.GetComponentInChildren<MeshRenderer>();
            
            Material _mat = GetMat();
            
            if (_mat.IsNull())
            {
                Debug.Log("Can not find material // or skinned mesh renderer");
                return;
            }

            InitializeHighlightVariables(_mat);
        }

        public async UniTask InitializeMeepleCharacterVisuals(CharacterStatsBase _data,
            ElementTyping _type, Transform _ballHoldPos)
        {
            
            characterModel = Instantiate(_data.characterModelAssetRef, modelParent.transform.position, modelParent.rotation);
            characterModel.transform.SetParent(modelParent);
            characterModel.transform.localPosition = Vector3.zero;
            
            meepleSkinnedMeshRenderers = characterModel.GetComponentsInChildren<SkinnedMeshRenderer>().ToNewList();
            meepleMeshRenderers = characterModel.GetComponentsInChildren<MeshRenderer>().ToNewList();

            if (meepleSkinnedMeshRenderers.Count == 0 && meepleMeshRenderers.Count == 0)
            {
                return;
            }

            m_clonedMaterial = new Material(meepleSkinnedMeshRenderers.Count > 0 ? meepleSkinnedMeshRenderers[0].materials[0]
                : meepleMeshRenderers.Count > 0 ? meepleMeshRenderers[0].materials[0] : default);

            meepleSkinnedMeshRenderers.ForEach(smr =>
            {
                smr.material = m_clonedMaterial;
            });
            
            meepleMeshRenderers.ForEach(mr =>
            {
                mr.material = m_clonedMaterial;
            });

            if (m_isChangeColor)
            {
                m_clonedMaterial.SetColor(LightColor, _type.meepleColors[0]);
                m_clonedMaterial.SetColor(DarkColor, _type.meepleColors[1]);     
            }
            
            InitializeHighlightVariables(m_clonedMaterial);
        }

        private Material GetMat()
        {
            if (m_skinnedMeshRenderer.IsNull() && m_meshRenderer.IsNull())
            {
                return default;
            }
            
            if (!m_skinnedMeshRenderer.IsNull())
            {
                return m_skinnedMeshRenderer.materials[0];
            }
            
            return m_meshRenderer.materials[0];
        }

        private void InitializeHighlightVariables(Material _associatedMaterial)
        {
            m_highlightAnimation.AssignNewTarget(characterModel.transform);
            
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
                characterModel.layer = _preferredLayer;
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