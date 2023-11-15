using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Perks;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class PerkController: GameControllerBase
    {
     
        #region Static

        public static PerkController Instance { get; private set; }

        #endregion
        
        #region Serialized Fields
        
        [SerializeField] private List<PerkBase> allPerks = new List<PerkBase>();
        
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

        public PerkBase GetPerkByGUID(string _searchGUID)
        {
            return allPerks.FirstOrDefault(pb => pb.perkGUID == _searchGUID);
        }
        
        #endregion
        
    }
}