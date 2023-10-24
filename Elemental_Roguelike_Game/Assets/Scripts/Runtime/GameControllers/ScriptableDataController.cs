using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class ScriptableDataController: GameControllerBase
    {
        
        #region Static

        public static ScriptableDataController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private List<CharacterSide> m_characterSides = new List<CharacterSide>();

        [SerializeField] private List<Status.Status> m_statuses = new List<Status.Status>();

        [SerializeField] private Status.Status m_fenceAppliedStatus;

        [SerializeField] private int m_fenceDamage = 10;

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

        public CharacterSide GetSideByGuid(string _searchGUID)
        {
            var side = m_characterSides.FirstOrDefault(cs => cs.sideGUID == _searchGUID);
            Debug.Log(side);
            return side;
        }

        public Status.Status GetFenceAppliedStatus()
        {
            return m_fenceAppliedStatus;
        }

        public int GetFenceDamage()
        {
            return m_fenceDamage;
        }

        #endregion
        
    }
}