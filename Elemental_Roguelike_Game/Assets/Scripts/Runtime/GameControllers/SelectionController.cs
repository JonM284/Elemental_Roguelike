using Project.Scripts.Utils;
using Runtime.Selection;
using UnityEngine;
using UnityEngine.EventSystems;
using Rewired;

namespace Runtime.GameControllers
{
    public class SelectionController : GameControllerBase
    {

        #region Serialized Fields

        [SerializeField] private LayerMask selectableLayers;

        #endregion

        #region Private Fields

        private float m_mouseDownTime;

        private float m_mouseInputThreshold = 0.1f;

        private int playerID = 0;

        private Player m_player;

        private UnityEngine.Camera m_mainCamera;

        #endregion

        #region Accessors

        public ISelectable currentSelectable { get; private set; }
        
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
            if (m_player.GetButtonDown("Confirm"))
            {
                m_mouseDownTime = Time.time;
            }

            if (m_player.GetButtonUp("Confirm"))
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

            var selectable = hit.collider.GetComponent<ISelectable>();

            if (selectable == null)
            {
                Debug.LogError($"No Selectable // Name:{hit.collider.name}", hit.collider);
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
                    currentSelectable.OnUnselected();
                }
                currentSelectable = selectable;
            }
            
            selectable.OnSelect();
            Debug.Log("<color=#00ff00>Selected</color>");

        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            m_player = ReInput.players.GetPlayer(playerID);
            base.Initialize();
        }

        #endregion
        
        
    }
}