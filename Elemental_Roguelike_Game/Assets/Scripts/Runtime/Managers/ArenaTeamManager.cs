using System;
using System.Collections.Generic;
using Data.Sides;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace Runtime.Managers
{
    public class ArenaTeamManager: MonoBehaviour
    {

        #region Read-Only

        private readonly string ballTag = "BALL";

        #endregion

        #region Actions

        public static Action<CharacterSide> GoalScored;

        #endregion

        #region Serialized Fields

        [SerializeField] private CharacterSide m_characterSide;

        [SerializeField] private List<Transform> m_startPositions = new List<Transform>();

        [SerializeField] private Transform m_goalPosition;

        [SerializeField] private VFXPlayer goalVFXPlayer;

        #endregion

        #region Accessors

        public CharacterSide characterSide => m_characterSide;

        public List<Transform> startPositions => m_startPositions;

        public Transform goalPosition => m_goalPosition;

        #endregion

        #region Unity Events

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(ballTag))
            {   
                other.TryGetComponent(out BallBehavior ballBehavior);
                ballBehavior.ForceStopBall();
                ballBehavior.gameObject.SetActive(false);
                ScoreGoal();
            }
        }

        #endregion


        #region Class Implementation

        private void ScoreGoal()
        {
            //goalVFXPlayer.PlayAt(goalPosition.position, Quaternion.identity);
            WinConditionController.Instance.GoalScored(m_characterSide);
            TurnController.Instance.ResetField(m_characterSide);
        }

        #endregion


    }
}