using System;
using System.Collections;
using Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.UI.DataReceivers;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Runtime.GameControllers
{
    public class JuiceController: GameControllerBase
    {

        #region Static

        public static JuiceController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private UIWindowData juiceUIWindowData;

        #endregion
        
        #region Private Fields

        private JuiceUIDataModel juiceUIDataModel;

        #endregion

        #region Unity Events

        public void OnEnable()
        {
            TurnController.OnBattlePreStart += SetupJuiceUI;
        }

        private void OnDisable()
        {
            TurnController.OnBattlePreStart -= SetupJuiceUI;
        }

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        private void SetupJuiceUI()
        {
            UIController.Instance.AddUICallback(juiceUIWindowData, InitializeJuiceUI);
        }

        private void InitializeJuiceUI(GameObject _uiWindowGO)
        {
            _uiWindowGO.TryGetComponent(out JuiceUIDataModel uiDataModel);
            if (uiDataModel.IsNull())
            {
                return;
            }
            
            juiceUIDataModel = uiDataModel;
            Debug.Log("Has Created Juice UI", juiceUIDataModel);
            
        }


        public IEnumerator DoReactionAnimation(int _endValueL, int _endValueR)
        {
            yield return StartCoroutine(juiceUIDataModel.C_ReactionEvent(_endValueL, _endValueR));
        }

        #endregion

    }
}