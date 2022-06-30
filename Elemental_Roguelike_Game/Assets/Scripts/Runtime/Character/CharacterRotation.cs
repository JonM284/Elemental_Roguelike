using UnityEngine;

public class CharacterRotation : MonoBehaviour
{

    #region Serialized Fields

    [SerializeField] private Transform playerModel;

    [SerializeField] private float rotationSpeed;

    #endregion

    #region Accessor

    private CharacterMovement _characterMovement => this.GetComponent<CharacterMovement>();

    private CharacterController _characterController => this.GetComponent<CharacterController>();

    #endregion

    // Update is called once per frame
    void Update()
    {
        HandleRotation();
    }

    void HandleRotation()
    {
        ///change forward direction
        Vector3 tempDir = new Vector3(_characterMovement.velocity.x, 0, _characterMovement.velocity.z);
        
        if (_characterController.isGrounded)
        {
            playerModel.forward = Vector3.Slerp(playerModel.forward, tempDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }
}
