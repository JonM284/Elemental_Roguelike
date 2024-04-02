using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.Character
{
    public class CharacterRotation : MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private Transform playerModel;

        [SerializeField] private float rotationSpeed;

        #endregion

        #region Private Fields

        private CharacterMovement m_characterMovement;

        private CharacterController m_characterController;

        private Vector3 m_targetPos;

        private Vector3 oldVel;

        private bool m_isInRotation;

        private float rotationTimer;

        #endregion

        #region Accessor

        private CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement, GetComponent<CharacterMovement>);

        private CharacterController characterController => CommonUtils.GetRequiredComponent(ref m_characterController, GetComponent<CharacterController>);

        #endregion

        // Update is called once per frame
        void Update()
        {
            HandleRotation();

            RotateToTarget();
        }

        #region Class Implementation

        void HandleRotation()
        {
            ///change forward direction
            Vector3 tempDir = new Vector3(characterMovement.velocity.x, 0, characterMovement.velocity.z);

            if (tempDir == oldVel)
            {
                return;
            }
            
            if (characterController.isGrounded)
            {
                playerModel.forward = Vector3.Slerp(playerModel.forward, tempDir.normalized, Time.deltaTime * rotationSpeed);
                oldVel = tempDir;
            }
        }

        void RotateToTarget()
        {
            if (!m_isInRotation)
            {
                return;
            }
            
            Vector3 tempDir = m_targetPos - transform.position;

            rotationTimer += Time.deltaTime;
            var per = rotationTimer / 0.3f;
            if (per < 1)
            {
                playerModel.forward = Vector3.Slerp(playerModel.forward, tempDir.normalized, per);
            }else if (per >= 1)
            {
                playerModel.forward = tempDir;
                m_isInRotation = false;
            }
            
        }

        public void SetRotationTarget(Vector3 _targetPos)
        {
            m_isInRotation = true;
            rotationTimer = 0;
            m_targetPos = new Vector3(_targetPos.x, transform.position.y, _targetPos.z);
        }

        #endregion


    }
}
