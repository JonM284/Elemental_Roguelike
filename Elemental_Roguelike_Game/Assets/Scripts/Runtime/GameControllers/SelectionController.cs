using Project.Scripts.Utils;
using Runtime.Selection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.GameControllers
{
    public class SelectionController: GameControllerBase
    {

        #region Events

        

        #endregion

        #region Serialized Fields

        [SerializeField] private LayerMask selectableLayers;

        #endregion

        #region Private Fields

        private float m_mouseDownTime;

        private float m_mouseInputThreshold = 0.2f;

        private UnityEngine.Camera m_mainCamera;

        #endregion

        #region Accessors

        public Selectable currentSelectable { get; private set; }

        public UnityEngine.Camera mainCamera => CommonUtils.GetRequiredComponent(ref m_mainCamera, () =>
        {
            var c = UnityEngine.Camera.main;
            return c;
        });

        #endregion
        
        #region Unity Events

        private void Update()
        {
            if (!is_Initialized)
            {
                return;
            }
            
            CheckUserInput();
        }

        #endregion
        
        #region Class Implementation

        private void CheckUserInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_mouseDownTime = Time.time;
            }

            if (Input.GetMouseButtonUp(0))
            {
                var _timeFromPress = Time.time - m_mouseDownTime;
                if (_timeFromPress <= m_mouseInputThreshold)
                {
                    TrySelect();
                }
            }
        }

        private void TrySelect()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            Debug.Log("TrySelect");
            if (mainCamera == null)
            {
                Debug.LogError("No Camera");
                return;
            }

            if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1000, selectableLayers))
            {
                Debug.LogError("No Hit Detected");
                return;
            }

            var selectable = hit.collider.GetComponent<Selectable>();

            if (selectable == null)
            {
                Debug.LogError("No Selectable");
                return;
            }

            if (selectable is NavigationSelectable navSelectable)
            {
                navSelectable.SelectPathingLocation(hit.point);
                return;
            }

            if (currentSelectable != selectable)
            {
                if (currentSelectable != null)
                {
                    currentSelectable.OnUnselect();
                }
                currentSelectable = selectable;
            }
            
            selectable.OnSelect();
            Debug.Log("<color=#00ff00>Selected</color>");

        }

        #endregion
        
        
    }
}