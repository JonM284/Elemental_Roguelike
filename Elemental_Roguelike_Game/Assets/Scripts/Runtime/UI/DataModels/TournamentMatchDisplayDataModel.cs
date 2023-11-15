using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.DataModels
{
    public class TournamentMatchDisplayDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIWindowDialog windowDialog;

        [SerializeField] private Image playerTeamCaptain;

        [SerializeField] private Image enemyTeamCaptain;

        [SerializeField] private TMP_Text tournamentText;

        #endregion

        #region Accessors

        private TournamentController tournamentController => TournamentController.Instance;

        private TournamentData selectedTournament => tournamentController.selectedTournament;

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            Initialize();
        }

        #endregion

        #region Class Implementation

        private void Initialize()
        {
            tournamentText.text = tournamentController.selectedTournament.tournamentName;

            var currentEnemyTeam = tournamentController.GetCurrentEnemyTeam();
            var currentPlayerTeam = TeamController.Instance.GetTeam();

            var playerCaptain = currentPlayerTeam.FirstOrDefault(csb => csb.m_characterStatsBase.isCaptain);
            var enemyCaptain = currentEnemyTeam.enemyCharacters.FirstOrDefault(csb => csb.isCaptain);

            if (!playerCaptain.IsNull())
            {
                if (!playerCaptain.m_characterStatsBase.characterImage.IsNull())
                {
                    playerTeamCaptain.sprite = playerCaptain.m_characterStatsBase.characterImage;
                }
            }

            if (!enemyCaptain.IsNull())
            {
                if (!enemyCaptain.characterImage.IsNull())
                {
                    enemyTeamCaptain.sprite = enemyCaptain.characterImage;
                }
            }
        }

        public void OnConfirm()
        {
            var randomLevel =
                selectedTournament.possibleArenaPools[Random.Range(0, selectedTournament.possibleArenaPools.Count)];
            SceneController.Instance.LoadScene(randomLevel, true);
            windowDialog.Close();
        }

        public void OnBack()
        {
            windowDialog.Close();
        }

        #endregion


    }
}