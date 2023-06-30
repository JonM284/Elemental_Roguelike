using System.Collections.Generic;
using Data.Sides;
using UnityEngine;

namespace Runtime.Managers
{
    public class ArenaTeamManager: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private CharacterSide m_characterSide;

        [SerializeField] private List<Transform> m_startPositions = new List<Transform>();

        #endregion

        #region Accessors

        public CharacterSide characterSide => m_characterSide;

        public List<Transform> startPositions => m_startPositions;

        #endregion


    }
}