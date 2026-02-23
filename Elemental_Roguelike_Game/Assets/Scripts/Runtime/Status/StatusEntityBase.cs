using System;
using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Status
{
    public abstract class StatusEntityBase: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] protected StatusData statusData;

        #endregion
        
        public int statusTimeMax { get; set; }
        
        public int statusTimeCurrent { get; set; }
        
        public CharacterBase currentOwner { get; set; }

        public bool isInitialized { get; set; }

        #region Unity Events

        private void OnEnable()
        {
            TurnController.OnChangeActiveTeam += OnTick;
        }

        private void OnDisable()
        {
            TurnController.OnChangeActiveTeam += OnTick;
        }

        #endregion

        #region Class Implementation

        public virtual void OnApply(CharacterBase characterBase)
        {
            if (characterBase.IsNull())
            {
                return;
            }

            currentOwner = characterBase;
            
            statusTimeMax = statusData.amountOfTurns;
            statusTimeCurrent = 0;
            
            isInitialized = true;
        }

        public abstract void OnTick(CharacterSide characterSide);

        public virtual void OnEnd()
        {
            isInitialized = false;
            currentOwner = null;
        }

        public StatusData GetStatusData()
        {
            return statusData;
        }

        public string GetGUID()
        {
            return statusData.statusIdentifierGUID;
        }

        #endregion
        
    }
}