using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Data.Sides;
using Project.Scripts.Utils;
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

        #region Nested Classes

        [Serializable]
        public class CharacterReactor
        {
            public CharacterClassManager classManager;
            public Action reactionToPerform;
        }

        #endregion

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

        private List<CharacterReactor> m_queuedReactions = new List<CharacterReactor>();

        private bool m_isScoring;

        private Coroutine m_reactionCoroutine;
        
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

        public void QueueReaction(CharacterClassManager _newQueuer, Action callback)
        {
            if (_newQueuer.IsNull())
            {
                return;
            }

            var _reactor = new CharacterReactor
            {
                classManager   = _newQueuer,
                reactionToPerform = callback
            };


            if (!m_queuedReactions.Contains(_reactor))
            {
                m_queuedReactions.Add(_reactor);
            }

            if (m_reactionCoroutine.IsNull())
            {
                m_reactionCoroutine = StartCoroutine(C_AllowCharacterPerformReaction());
            }
        }

        private void EndReactionQueue()
        {
            StopCoroutine(m_reactionCoroutine);
            m_reactionCoroutine = null;
        }

        private IEnumerator C_AllowCharacterPerformReaction()
        {
            yield return null;

            while (m_queuedReactions.Count > 0)
            {
                yield return null;

                var currentReactor = m_queuedReactions[0];
                
                currentReactor.reactionToPerform?.Invoke();
                yield return new WaitUntil(() => !currentReactor.classManager.isPerformingReaction);

                m_queuedReactions.Remove(currentReactor);
            }
            
            EndReactionQueue();
            
        }

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