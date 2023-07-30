using System;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.Selection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Submodules
{
    public class HighlightHelper: MonoBehaviour
    {
        
        #region Serialized Fields

        [SerializeField] private LayerMask selectableLayers;

        #endregion

        #region Private Fields

        private UnityEngine.Camera m_mainCamera;

        #endregion

        #region Accessors

        private CharacterBase activeCharacter => TurnController.Instance.activeCharacter;

        private bool isPlayerTurn => TurnController.Instance.isPlayerTurn;
        
        public ISelectable currentHovered { get; private set; }

        public UnityEngine.Camera mainCamera => CommonUtils.GetRequiredComponent(ref m_mainCamera, () =>
        {
            var c = UnityEngine.Camera.main;
            return c;
        });

        #endregion

        #region Unity Events

        private void Update()
        {
            if (activeCharacter.IsNull())
            {
                CheckHover();
                return;
            }

            if (!activeCharacter.IsNull() && isPlayerTurn)
            {
                CheckPlayerAction();
            }
        }

        #endregion

        #region Class Implementation
        

        private void CheckPlayerAction()
        {
            if (activeCharacter.IsNull())
            {
                return;
            }
            
            if (!activeCharacter.isDoingAction)
            {
                return;
            }
            
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            if (mainCamera == null)
            {
                return;
            }

            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000, selectableLayers))
            {
                return;
            }

            hit.collider.TryGetComponent(out ISelectable selectable);

            if (selectable.IsNull())
            {
                return;
            }
            
            if (currentHovered != selectable)
            {
                if (currentHovered != null)
                {
                    currentHovered.OnUnHover();
                }
                currentHovered = selectable;
                selectable.OnHover();
            }
            
            activeCharacter.MarkHighlightArea(hit.point);
        }

        private void CheckHover()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                CheckHoveredObject();
                return;
            }
            
            if (mainCamera == null)
            {
                return;
            }

            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000, selectableLayers))
            {
                CheckHoveredObject();
                return;
            }

            hit.collider.TryGetComponent(out ISelectable selectable);

            if (selectable.IsNull())
            {
                CheckHoveredObject();
                return;
            }

            if (currentHovered != selectable)
            {
                CheckHoveredObject();
                currentHovered = selectable;
                selectable.OnHover();
            }

        }

        private void CheckHoveredObject()
        {
            if (currentHovered.IsNull())
            {
                return;
            }
            
            currentHovered.OnUnHover();
            currentHovered = null;
        }

        #endregion
        
        
    }
}