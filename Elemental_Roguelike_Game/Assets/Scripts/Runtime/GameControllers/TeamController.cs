using System.Collections.Generic;
using Data.DataSaving;
using Runtime.Character;

namespace Runtime.GameControllers
{
    public class TeamController: GameControllerBase, ISaveableData
    {

        #region Private Feilds

        private Team playerTeam;
        
        private List<Team> m_activeTeams = new List<Team>();

        #endregion

        #region Accessors

        public List<Team> activeTeams => m_activeTeams;

        #endregion

        #region Class Implementation

        public void AddTeam(Team _teamToAdd)
        {
            if (_teamToAdd == null || _teamToAdd.teamMembers.Count == 0)
            {
                return;
            }
            
            m_activeTeams.Add(_teamToAdd);
        }

        public void RemoveTeam(Team _teamToRemove)
        {
            if (_teamToRemove == null || _teamToRemove.teamMembers.Count == 0)
            {
                return;
            }

            if (m_activeTeams.Contains(_teamToRemove))
            {
                m_activeTeams.Remove(_teamToRemove);
            }
            
        }

        public void RemoveAllTeams()
        {
            m_activeTeams.ForEach(t => m_activeTeams.Remove(t));
        }

        #endregion

        #region ISaveableData Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            this.playerTeam = _savedGameData.savedTeam;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.savedTeam = this.playerTeam;
        }

        #endregion
        
    }
}