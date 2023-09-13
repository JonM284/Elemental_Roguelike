﻿using System;
using Data;
using Data.Sides;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class WinConditionController : GameControllerBase
    {

        #region Static

        public static WinConditionController Instance { get; private set; }

        #endregion

        #region Actions

        public static event Action<bool> ReportMatchResults;

        public static event Action PointThresholdReached;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private CharacterSide playerSide;

        [SerializeField] private int pointsToWin;

        [SerializeField] private UIWindowData endMatchData;
        
        #endregion

        #region Private Fields

        private bool m_isPlayerVictory;

        private bool m_isGameConditionMet;

        #endregion
        
        #region Accessors

        public int redTeamPoints { get; private set; }
        
        public int blueTeamPoints { get; private set; }

        public bool isGameOver => m_isGameConditionMet;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnBattleStarted += TurnControllerOnOnBattleStarted;
            TurnController.OnRunEnded += TurnController_OnRunEnded;
            SceneController.OnLevelFinishedLoading += OnLevelFinishedLoading;
        }

        private void OnDisable()
        {
            TurnController.OnBattleStarted -= TurnControllerOnOnBattleStarted;
            TurnController.OnRunEnded -= TurnController_OnRunEnded;
            SceneController.OnLevelFinishedLoading -= OnLevelFinishedLoading;
        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion


        #region Class Implementation
        
        private void TurnControllerOnOnBattleStarted()
        {
            m_isGameConditionMet = false;
        }

        private void TurnController_OnRunEnded()
        {
            if (!is_Initialized)
            {
                return;
            }
            //Game Over
        }

        private void CheckPointThreshold()
        {
            if (redTeamPoints < pointsToWin && blueTeamPoints < pointsToWin)
            {
                return;
            }

            if (redTeamPoints >= pointsToWin)
            {
                //red win
                m_isPlayerVictory = false;
                Debug.Log("PLAYER LOSE");
            }else if (blueTeamPoints >= pointsToWin)
            {
                //blue win
                m_isPlayerVictory = true;
                Debug.Log("PLAYER WIN");
            }
            
            //ToDo: End Match Screen
            m_isGameConditionMet = true;
            Debug.Log("INVOKING POINT THRESHOLD ACTIOn");
            PointThresholdReached?.Invoke();
            UIUtils.OpenUI(endMatchData);
        }
        
        private void OnLevelFinishedLoading(SceneName _name, bool _isMatchScene)
        {
            if (_isMatchScene)
            {
                ResetPoints();
            }
            else
            {
                if (_name == SceneName.RealWorldScene)
                {
                    Debug.Log("REPORT MATCH RESULTS");
                    ReportMatchResults?.Invoke(m_isPlayerVictory);
                }
            }
            
        }

        private void ResetPoints()
        {
            redTeamPoints = 0;
            blueTeamPoints = 0;
        }

        public void GoalScored(CharacterSide _side)
        {
            if (_side.IsNull())
            {
                return;
            }

            //If the side of the goal isn't player side, then player point
            if (_side != playerSide)
            {
                blueTeamPoints++;
                CheckPointThreshold();
                Debug.Log("<color=cyan>Player TEAM SCORE</color>");
                return;
            }
            
            //If the side is player side, enemy goal
            Debug.Log("<color=red>ENEMY TEAM SCORE</color>");
            redTeamPoints++;
            CheckPointThreshold();

        }

        #endregion
        
        
    }
}