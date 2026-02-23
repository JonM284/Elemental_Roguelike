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

        public static readonly float MaxGravityPullConstant = 1.2f;
        public static readonly float MinGravityPullConstant = 0.4f;

        public static readonly float GravDistClampMin = 7f;
        public static readonly float GravDistClampMax = 20f;

        public static readonly float PassiveDistMin = 1f;
        public static readonly float PassiveDistMaxAddition = 5f;
        public static readonly float PassiveDistMax = 6f;

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