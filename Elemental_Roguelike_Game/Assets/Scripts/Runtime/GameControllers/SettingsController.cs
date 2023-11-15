using Data;
using Data.Sides;
using Project.Scripts.Utils;
using UnityEngine;

namespace Runtime.GameControllers
{
    public class SettingsController: GameControllerBase
    {
        
        #region Static

        public static SettingsController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private SettingsData _settingsData;

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

        public Color GetSideColor(string _characterSideGUID)
        {
            if (string.IsNullOrEmpty(_characterSideGUID))
            {
                return Color.white;
            }

            if (_characterSideGUID == TurnController.Instance.playersSide.sideGUID)
            {
                return _settingsData.playerSideColor;
            }else
            {
                return _settingsData.enemySideColor;
            }

        }

        public ColorblindOptions GetCurrentColorblindOption()
        {
            return _settingsData.colorblindOptions;
        }

        #endregion
        
        
        
        
    }
}