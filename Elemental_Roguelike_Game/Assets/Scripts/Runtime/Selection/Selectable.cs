using System;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Selection
{
    public class Selectable: MonoBehaviour, ISelectable
    {

        #region Events

        public UnityEvent onSelect;
        
        public UnityEvent onUnselect;
        
        public UnityEvent onHover;

        public UnityEvent onUnhover;

        #endregion

        #region Class Implementation

        public void OnSelect()
        {
            onSelect?.Invoke();
        }

        public void OnUnselected()
        {
            onUnselect?.Invoke();
        }

        public void OnHover()
        {
            onHover?.Invoke();
        }

        public void OnUnHover()
        {
            onUnhover?.Invoke();
        }

        #endregion

    }
}