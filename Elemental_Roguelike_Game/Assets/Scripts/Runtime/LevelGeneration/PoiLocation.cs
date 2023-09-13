using System;
using Project.Scripts.Utils;
using Runtime.GameplayEvents;
using Runtime.Selection;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    [Serializable]
    public class PoiLocation: MonoBehaviour, ISelectable
    {

        #region Read-Only

        private static readonly int mainColor = Shader.PropertyToID("_MainColor");

        #endregion

        #region Actions

        public static event Action<PoiLocation, GameplayEventType> POILocationSelected;

        #endregion

        #region Serialized Fields

        [SerializeField] private MeshRenderer eventQuad;

        #endregion

        #region Private Fields

        private Material m_materialRef;

        private Material m_clonedMaterial;

        private bool m_isActive;
        
        private Color m_activeColor = Color.white;

        private Color m_highlightColor = Color.yellow;

        private Color m_selectedColor = Color.green;

        private Color m_normalColor = Color.grey;

        private Color m_inactiveColor = Color.black;

        #endregion

        #region Accessors

        private Material objMaterial => CommonUtils.GetRequiredComponent(ref m_materialRef, () =>
        {
            var m = this.GetComponent<MeshRenderer>().materials[0];
            return m;
        });

        public GameplayEventType AssignedEventType { get; private set; }

        public Vector3 savedLocation { get; private set; }

        #endregion
        
        #region Class Implementation

        public void Initialize(GameplayEventType _event)
        {
            m_clonedMaterial = new Material(objMaterial);

            savedLocation = transform.localPosition;

            this.GetComponent<MeshRenderer>().material = m_clonedMaterial;

            m_clonedMaterial.SetColor(mainColor, m_normalColor);

            if (!eventQuad.IsNull())
            {
                if (!_event.eventTexture.IsNull())
                {
                    eventQuad.materials[0].mainTexture = _event.eventTexture;
                }
            }

            AssignedEventType = _event;
        }

        public void SetPointActive()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }

            m_isActive = true;
            m_clonedMaterial.SetColor(mainColor, m_activeColor);
        }

        public void SetPointSelected()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }

            m_isActive = false;
            m_clonedMaterial.SetColor(mainColor, m_selectedColor);
        }

        private void SetPointHighlight(bool _highlight)
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }

            var applyColor = _highlight ? m_highlightColor : m_activeColor;
               m_clonedMaterial.SetColor(mainColor, applyColor);
        }

        public void SetPointInactive()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }

            m_isActive = false;
            m_clonedMaterial.SetColor(mainColor, m_inactiveColor);
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            if (!m_isActive)
            {
                return;
            }
            SetPointSelected();
            POILocationSelected?.Invoke(this, AssignedEventType);
        }

        public void OnUnselected()
        {
            
        }

        public void OnHover()
        {
            if (!m_isActive)
            {
                return;
            }
            SetPointHighlight(true);
        }

        public void OnUnHover()
        {
            if (!m_isActive)
            {
                return;
            }
            SetPointHighlight(false);
        }

        #endregion
        
    }
}