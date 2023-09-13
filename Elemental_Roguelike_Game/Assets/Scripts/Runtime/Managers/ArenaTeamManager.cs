using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Runtime.Character;
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

        #region Serialized Fields

        [SerializeField] private CharacterSide m_characterSide;

        [SerializeField] private List<Transform> m_startPositions = new List<Transform>();

        [SerializeField] private Transform m_goalPosition;

        [SerializeField] private VFXPlayer goalVFXPlayer;

        #endregion

        #region Private Fields

        private bool m_isScoring;

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
                if (!m_isScoring)
                {
                    m_isScoring = true;
                    StartCoroutine(C_ScoreGoal(ballBehavior));
                }
            }
        }

        #endregion


        #region Class Implementation

    

        private IEnumerator C_ScoreGoal(BallBehavior _ball)
        {
            var isPlayersGoal = characterSide == TurnController.Instance.playersSide;

            var lastContactedPlayer = _ball.lastContactedCharacters.LastOrDefault(cb => cb.side != characterSide);
            lastContactedPlayer.characterClassManager.ChangeToDisplayLayer();
            
            CameraUtils.SetCameraZoom(0.3f);
            CameraUtils.SetCameraTrackPos(lastContactedPlayer.transform, false);

            TurnController.Instance.HaltAllPlayers();
            
            var cameraPosition = lastContactedPlayer.characterClassManager.reactionCameraPoint;

            yield return StartCoroutine(JuiceController.Instance.C_ScorePoint(isPlayersGoal, cameraPosition));
            
            lastContactedPlayer.characterClassManager.ChangeToNormalLayer();

            //goalVFXPlayer.PlayAt(goalPosition.position, Quaternion.identity);
            WinConditionController.Instance.GoalScored(m_characterSide);
            
            Debug.Log("SCORE GOAL");

            m_isScoring = false;
            
            if (!WinConditionController.Instance.isGameOver)
            {
                TurnController.Instance.ResetField(m_characterSide);
            }
        }

        #endregion


    }
}