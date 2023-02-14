using System;
using UnityEngine;

namespace Runtime.Camera
{
    public class CameraRotationTracker: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private float rotationSpeed;

        #endregion

        #region Private Fields

        private Vector2 m_dragStartPos;

        private Vector2 m_dragCurrentPos;

        private Quaternion m_newRotation = Quaternion.identity;

        #endregion

        #region Unity Events

        private void LateUpdate()
        {
            HandleRotation();
            transform.rotation = Quaternion.Lerp(transform.rotation, m_newRotation, rotationSpeed * Time.deltaTime);
        }

        #endregion
        
        #region Class Implementation

        private void HandleRotation()
        {

            if (Input.GetMouseButtonDown(1))
            {
                m_dragStartPos = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {
                m_dragCurrentPos = Input.mousePosition;

                var _dir = m_dragStartPos - m_dragCurrentPos;

                m_dragStartPos = m_dragCurrentPos;

                m_newRotation *= Quaternion.Euler(Vector3.up * (-_dir.x / 5f));
            }
            
        }

        #endregion
        
    }
}