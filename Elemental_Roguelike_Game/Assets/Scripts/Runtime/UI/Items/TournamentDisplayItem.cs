using System;
using Data;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using Utils;

namespace Runtime.UI.Items
{
    public class TournamentDisplayItem : MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private TMP_Text nameText;

        [SerializeField] private GameObject highlightImage;

        #endregion

        #region Private Fields

        private Action<TournamentData> m_pressedAction;

        #endregion

        #region Accessors

        public TournamentData assignedData { get; private set; }

        #endregion
        
        #region Class Implementation

        public void Initialize(TournamentData _data, Action<TournamentData> _pressedAction)
        {
            if (_data.IsNull())
            {
                return;
            }

            assignedData = _data;

            nameText.text = assignedData.tournamentName;

            m_pressedAction = _pressedAction;
        }

        public void OnTournamentSelect()
        {
            m_pressedAction?.Invoke(assignedData);
            m_pressedAction = null;
        }

        public void CheckHighlight(bool _isHighlighted)
        {
            highlightImage.SetActive(_isHighlighted);
        }

        #endregion
        
        
    }
}