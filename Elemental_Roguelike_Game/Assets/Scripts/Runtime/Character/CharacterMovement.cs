using Project.Scripts.Data;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    #region Private Fields

    private Vector3 _velocity;

    #endregion

    #region SerializedFields

    [SerializeField] private CharacterStatsBase characterStats;

    [SerializeField] private float gravity;

    [Header("Ground Checker")] 
    
    [SerializeField]
    private float distance;

    [SerializeField] private float radius;

    #endregion

    #region Accessors

    private float speed => characterStats != null ? characterStats.baseSpeed : 1f;

    private float maxGravity => gravity * 20f;

    private CharacterController _characterController => this.GetComponent<CharacterController>();

    public Vector3 velocity => _velocity;

    #endregion


    #region Class Implementation

    // Update is called once per frame
    void Update()
    {
        if (_characterController.isGrounded)
        {
            Move();
        }
        else
        {
            DoGravity();
        }
    }

    private void DoGravity()
    {
        _velocity.y -= gravity * Time.deltaTime;
        
        _characterController.Move(Vector3.ClampMagnitude(_velocity, maxGravity) * Time.deltaTime);
    }

    private void Move()
    {
        _velocity.x = Input.GetAxisRaw("Horizontal") * speed;
        _velocity.y = 0;
        _velocity.z = Input.GetAxisRaw("Vertical") * speed;

        _characterController.Move(Vector3.ClampMagnitude(_velocity, speed) * Time.deltaTime);
    }

    #endregion

   
}
