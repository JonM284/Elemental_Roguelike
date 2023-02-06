using System;
using Project.Scripts.Data;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    

    #region SerializedFields

    [SerializeField] private CharacterStatsBase characterStats;

    [SerializeField] private float gravity;

    #endregion

    #region Private Fields

    private Vector3 _velocity;

    private bool m_isInBattle;

    private bool m_canMove = true;

    private Camera m_mainCamera;

    private CharacterController m_characterController;

    private NavMeshAgent m_navMeshAgent;

    #endregion
    
    #region Accessors

    private float speed => characterStats != null ? characterStats.baseSpeed : 1f;

    public float battleMoveDistance => characterStats != null ? characterStats.movementDistance : 1f;

    public float maxGravity => gravity * 20f;

    private CharacterController characterController => CommonUtils.GetRequiredComponent(ref m_characterController, () =>
    {
        var cc = GetComponent<CharacterController>();
        return cc;
    });

    private NavMeshAgent navMeshAgent => CommonUtils.GetRequiredComponent(ref m_navMeshAgent, () =>
    {
        var nma = GetComponent<NavMeshAgent>();
        return nma;
    });
    
    public Camera mainCamera => CommonUtils.GetRequiredComponent(ref m_mainCamera, () =>
    {
        var c = Camera.main;
        return c;
    });

    public Vector3 velocity => _velocity;
    
    public Vector3 pivotPosition { get; private set; }
    
    public Vector3 relativeRight { get; private set; }

    public Vector3 relativeForward { get; private set; }

    #endregion

    #region Unity Events

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, battleMoveDistance);
    }

    void Update()
    {
       HandleMovement();
    }

    #endregion
    
    #region Class Implementation

    private void HandleMovement()
    {
        if (!m_canMove)
        {
            return;
        }
        
        
        
    }

    public void MoveCharacterAgent(Vector3 _movePosition)
    {
        navMeshAgent.SetDestination(_movePosition);
    }

    [ContextMenu("Battle On")]
    public void SetCharacterBattleStatus()
    {
        m_isInBattle = true;
    }

    [ContextMenu("Active char")]
    public void SetCharacterActiveCharacter()
    {
        pivotPosition = transform.position.FlattenVector3Y();
        m_canMove = true;
    }

    public void SetCharacterInactive()
    {
        m_canMove = false;
        TeleportCharacter(pivotPosition);
    }

    public void TeleportCharacter(Vector3 teleportPosition)
    {
        characterController.enabled = false;
        transform.position = teleportPosition;
        characterController.enabled = true;
    }

    #endregion

    #region Unused

    /* This is used if going back to character controller movement
     
    private void InitializeCharacterMovement()
    {
        relativeRight = mainCamera.transform.right.normalized.FlattenVector3Y();
        relativeForward = mainCamera.transform.forward.normalized.FlattenVector3Y();
    }
     
    private void DoGravity()
    {
        _velocity.y -= gravity * Time.deltaTime;
        
        _characterController.Move(Vector3.ClampMagnitude(_velocity, maxGravity) * Time.deltaTime);
    }

    private void FreeMove()
    {
        var _xInput = Input.GetAxisRaw("Horizontal") * speed;
        var _yInput = Input.GetAxisRaw("Vertical") * speed;
        
        _velocity = Vector3.Normalize(relativeRight * _xInput + relativeForward * _yInput).FlattenVector3Y();
        _velocity *= speed * Time.deltaTime;

        _characterController.Move(Vector3.ClampMagnitude(_velocity, speed));
    }

    private void BattleMove()
    {
        FreeMove();
        float mag = Vector3.Magnitude(pivotPosition - transform.position);
        if (mag > battleMoveDistance)
        {
            var dirFromCenter = (transform.position - pivotPosition).normalized;
            var farthestPointInDir = dirFromCenter * battleMoveDistance;
            TeleportCharacter(farthestPointInDir);
        }
    }
    */

    #endregion

   
}
