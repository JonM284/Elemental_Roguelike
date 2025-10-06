using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.GameControllers
{
    public class ScriptableDataController: GameControllerBase
    {
        
        #region Static

        public static ScriptableDataController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private List<CharacterSide> m_characterSides = new List<CharacterSide>();

        [SerializeField] private List<Status.StatusEntityBase> m_statuses = new List<Status.StatusEntityBase>();

        [FormerlySerializedAs("mFenceAppliedStatusBase")] [FormerlySerializedAs("m_fenceAppliedStatus")] [SerializeField] private Status.StatusEntityBase mFenceAppliedStatusEntityBase;

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
            Debug.Log(side.name);
            return side;
        }

        public Status.StatusEntityBase GetFenceAppliedStatus()
        {
            return mFenceAppliedStatusEntityBase;
        }

        public int GetFenceDamage()
        {
            return m_fenceDamage;
        }

        #endregion
        
    }
}