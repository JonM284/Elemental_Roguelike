using System;
using Project.Scripts.Utils;
using Runtime.GameplayEvents;
using Runtime.Selection;
using UnityEngine;

namespace Project.Scripts.Runtime.LevelGeneration
{
    public class PoiLocation: MonoBehaviour, ISelectable
    {

        #region Read-Only

        private static readonly int mainColor = Shader.PropertyToID("_MainColor");

        #endregion

        #region Actions

        public static event Action<PoiLocation, GameplayEventType> POILocationSelected;

        #endregion

        #region Private Fields

        private Material m_materialRef;

        private Material m_clonedMaterial;

        #endregion

        #region Accessors

        private Material objMaterial => CommonUtils.GetRequiredComponent(ref m_materialRef, () =>
        {
            var m = this.GetComponent<MeshRenderer>().materials[0];
            return m;
        });

        public GameplayEventType AssignedEventType { get; private set; }

        #endregion
        
        #region Class Implementation

        public void Initialize(GameplayEventType _event)
        {
            m_clonedMaterial = new Material(objMaterial);

            this.GetComponent<MeshRenderer>().material = m_clonedMaterial;

            AssignedEventType = _event;
        }

        public void SetPointActive()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }
            
            m_clonedMaterial.SetColor(mainColor, Color.white);
        }

        private void SetPointSelected()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }
            
            m_clonedMaterial.SetColor(mainColor, Color.green);
        }

        private void SetPointHighlight(bool _highlight)
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }

            var applyColor = _highlight ? Color.yellow : Color.gray;
               m_clonedMaterial.SetColor(mainColor, applyColor);
        }

        public void SetPointInactive()
        {
            if (m_clonedMaterial.IsNull())
            {
                return;
            }
            
            m_clonedMaterial.SetColor(mainColor, Color.grey);
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            SetPointSelected();
            POILocationSelected?.Invoke(this, AssignedEventType);
        }

        public void OnUnselected()
        {
            
        }

        public void OnHover()
        {
            SetPointHighlight(true);
        }

        public void OnUnHover()
        {
            SetPointHighlight(false);
        }

        #endregion
        
    }
}