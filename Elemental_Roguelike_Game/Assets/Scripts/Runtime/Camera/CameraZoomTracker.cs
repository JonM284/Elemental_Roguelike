using System;
using UnityEngine;

namespace Runtime.Camera
{
    public class CameraZoomTracker: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private float scrollSpeed;

        [SerializeField] private float automaticZoomSpeed;

        [SerializeField] private float minZoom;

        [SerializeField] private float maxZoom;

        #endregion

        #region Private Fields

        private Vector3 m_localZoomZ;
        
        private float m_percentage;

        private float m_endValue;

        private float m_initialValue;

        private float m_timeToTravel;

        private float m_startTime;

        private bool m_changingValue;

        #endregion

        #region Accessors

        public Vector3 minPos => new Vector3(0, 0, minZoom);

        public Vector3 maxPos => new Vector3(0, 0, maxZoom);

        #endregion

        #region Unity Events

        private void Update()
        {
            if (!m_changingValue)
            {
                m_percentage = Mathf.Min(1, Mathf.Max(0, m_percentage + Input.mouseScrollDelta.y * scrollSpeed * -1));
            }
            
            if (m_changingValue) {
                var progress = (Time.time - m_startTime) / m_timeToTravel;
                if (progress <= 1) {
                    m_percentage = Mathf.Lerp(m_initialValue, m_endValue, progress);
                } else {
                    m_percentage = m_endValue;
                    m_changingValue = false;
                }
            }
            
            m_localZoomZ = Vector3.Lerp(minPos, maxPos, m_percentage);
        }

        private void LateUpdate()
        {
            TrackZoom();
        }

        #endregion

        #region Class Implementation

        private void TrackZoom()
        {
            var originalSpeed = Time.deltaTime * automaticZoomSpeed;
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, m_localZoomZ, originalSpeed);
        }

        public void SetNewValue(float _newPercentage)
        {
            m_timeToTravel = Mathf.Abs(( m_initialValue - m_endValue) / automaticZoomSpeed);
        }

        #endregion


    }
}