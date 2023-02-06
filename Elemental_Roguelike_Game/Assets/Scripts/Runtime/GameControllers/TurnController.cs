﻿using System.Collections.Generic;
using System.Linq;
using Runtime.Character;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class TurnController: GameControllerBase
    {

        #region Private Fields

        private int m_currentTeamTurnIndex = 0;
        
        private Team m_currentActiveTeam;
        
        #endregion

        #region Accessors

        public List<Team> activeTeams => TeamUtils.GetActiveTeams();

        public bool isTeamFinished => m_currentActiveTeam != null && m_currentActiveTeam.teamMembers.All(c => c.finishedTurn);

        public Team activeTeam => m_currentActiveTeam;

        public CharacterBase activeCharacter { get; private set; }

        #endregion

        #region Class Implementation

        public void StartBattle()
        {
            FlipCoin();
            SetAllTeamsStartBattle();
        }

        private void FlipCoin()
        {
            if (activeTeams.Count == 0)
            {
                return;
            }

            m_currentTeamTurnIndex = Random.Range(0, activeTeams.Count);
            SetTeamActive(activeTeams[m_currentTeamTurnIndex]);
        }

        private void SetAllTeamsStartBattle()
        {
            if (activeTeams.Count == 0)
            {
                return;
            }
            activeTeams.ForEach(t => t.teamMembers.ForEach(c => c.InitializeCharacterBattle()));
        }

        public void ChangeTeamTurn()
        {
            m_currentTeamTurnIndex++;
            if (m_currentTeamTurnIndex >= activeTeams.Count)
            {
                m_currentTeamTurnIndex = 0;
            }

            m_currentActiveTeam = activeTeams[m_currentTeamTurnIndex];

            SetTeamActive(m_currentActiveTeam);
        }

        private void SetTeamActive(Team _activeTeam)
        {
            _activeTeam.teamMembers.ForEach(c => c.ResetCharacterActions());
        }

        public void SetActiveCharacter(CharacterBase _newActiveCharacter)
        {
            if (_newActiveCharacter == null)
            {
                return;
            }

            /*if (!m_currentActiveTeam.teamMembers.Contains(_newActiveCharacter))
            {
                return;
            }*/

            activeCharacter = _newActiveCharacter;
        }

        #endregion
        
        
    }
}