using System;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Selection
{
    public class Selectable: MonoBehaviour
    {

        #region Events

        public UnityEvent onSelect;
        
        public UnityEvent onUnselect;
        
        public UnityEvent onHover;

        public UnityEvent onUnhover;

        #endregion

        #region Unity Events

        private void OnMouseEnter()
        {
            OnHover();
        }

        private void OnMouseExit()
        {
            OnUnhover();
        }

        #endregion

        #region Class Implementation

        public void OnSelect()
        {
            onSelect?.Invoke();
        }

        public void OnUnselect()
        {
            onUnselect?.Invoke();
        }

        public void OnHover()
        {
            onHover?.Invoke();
        }

        public void OnUnhover()
        {
            onUnhover?.Invoke();
        }

        #endregion

    }
}