using System.Collections.Generic;
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

        public static void AddTeam(Team _teamToAdd)
        {
            if (_teamToAdd == null || _teamToAdd.teamMembers.Count == 0)
            {
                return;
            }
            
            teamController.AddTeam(_teamToAdd);
        }

        public static List<Team> GetActiveTeams()
        {
            return teamController.activeTeams.ToList();
        }

        #endregion

    }
}