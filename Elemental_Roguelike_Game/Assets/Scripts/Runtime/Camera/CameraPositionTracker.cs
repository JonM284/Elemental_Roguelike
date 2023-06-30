using System;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using UnityEngine;
using Utils;

namespace Runtime.Camera
{
    public class CameraPositionTracker: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private float moveSpeed;

        [SerializeField] private float autoMoveSpeed;

        [SerializeField] private float radius;

        [SerializeField] private float padding;

        #endregion

        #region Private Fields

        private float m_timeToTravel;

        private float m_startTime;

        private bool m_isDrag;

        private bool m_isResettingCamera;
        
        private Vector3 m_centralPosition;

        private Vector3 m_dragStartPos;

        private Vector3 m_velocity;

        private Vector3 m_changeRoomStartPosition;

        private Plane m_dragPlane = new Plane(Vector3.up, Vector3.zero);

        #endregion

        #region Accessor

        public UnityEngine.Camera mainCamera => CameraUtils.GetMainCamera();

        Vector3 relativeRight => mainCamera.transform.right.normalized.FlattenVector3Y();
        
        Vector3 relativeForward => mainCamera.transform.forward.normalized.FlattenVector3Y();

        private Vector3 maxPosition => new Vector3(m_centralPosition.x + padding, 0, m_centralPosition.z + padding);
        
        private Vector3 minPosition => new Vector3(m_centralPosition.x - padding, 0, m_centralPosition.z - padding);
        

        #endregion

        #region Unity Events

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void LateUpdate()
        {
            if (!m_isResettingCamera)
            {
                TrackPositionByDrag();
            }
            else
            {
                RecenterCamera();
            }
        }

        #endregion

        #region Class Implementation
        

        //Check dragging input of the player (mouse)
        private void TrackPositionByDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_isDrag = true;
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                float entry;
                if (m_dragPlane.Raycast(ray, out entry))
                {
                    m_dragStartPos = ray.GetPoint(entry);
                }
            }

            if (Input.GetMouseButton(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                float entry;
                if (m_dragPlane.Raycast(ray, out entry))
                {
                    var dragCurrentPosition = ray.GetPoint(entry);
                    m_velocity = ConstrainRange(transform.position + m_dragStartPos - dragCurrentPosition);
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                m_isDrag = false;
            }

            transform.position = Vector3.Lerp(transform.position, m_velocity, moveSpeed * Time.deltaTime);

        }

        //Limit range to current room >> TODO:Add Radius to rooms, some rooms may be bigger
        private Vector3 ConstrainRange(Vector3 _inputVector)
        {
            //Change with current room position, unless camera changes parents
            Vector3 constrainedPos = Vector3.zero;

            var dir = constrainedPos - _inputVector;
            var mag = dir.magnitude;
            
            return new Vector3 {
                x = Mathf.Max(minPosition.x - radius, Math.Min(maxPosition.x + radius, _inputVector.x)),
                y = 0,
                z = Mathf.Max(minPosition.z - radius/2, Math.Min(maxPosition.z + radius/2, _inputVector.z)),
            };
        }

        
        //Automatically move camera to center
        private void RecenterCamera()
        {
            if (m_centralPosition.IsNan())
            {
                return;
            }
            
            var progress = (Time.time - m_startTime) / m_timeToTravel;
            if (progress <= 1) {
                m_velocity = Vector3.Lerp(m_changeRoomStartPosition, m_centralPosition, progress);
            } else {
                m_velocity = m_centralPosition;
                m_isResettingCamera = false;
            }

            transform.position = m_velocity;            
        }

        #endregion
    }
}