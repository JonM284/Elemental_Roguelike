using System.Collections.Generic;
using Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;

namespace Utils
{
    public static class TeamUtils
    {

        #region Private Fields

        private static TeamController m_teamController;

        #endregion

        #region Accessor

        public static TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);

        #endregion


        #region Class Implementation

        public static void AddTeamMember(CharacterBase _teamMember, CharacterStatsData _meepleData)
        {
            if (_teamMember == null)
            {
                return;
            }
            
            teamController.AddTeamMember(_teamMember, _meepleData);
        }
        
        public static void RemoveTeamMember(CharacterBase _teamMember, CharacterStatsData _meepleData)
        {
            if (_teamMember == null)
            {
                return;
            }
            
            teamController.RemoveTeamMember(_teamMember, _meepleData);
        }

        public static Team GetCurrentTeam()
        {
            return teamController.playerTeam;
        }

        #endregion

    }
}