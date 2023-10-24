using System;
using System.Collections;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.UI.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.UI.DataModels
{
    public class TournamentSelectionUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIWindowData characterSelectionData;

        [SerializeField] private UIWindowDialog tournamentDialog;

        [SerializeField] private AssetReference tournamentReference;

        [SerializeField] private Transform tournamentParent;

        #endregion

        #region Private Fields

        private GameObject loadedTournamentReference;

        #endregion

        #region Unity Events

        private IEnumerator Start()
        {
            yield return StartCoroutine(InitializeScreen());
        }

        #endregion
        
        #region Class Implementation

        private IEnumerator InitializeScreen()
        {
            var allTournaments = TournamentController.Instance.GetAllTournaments();

            //create items
            
            var handle = Addressables.LoadAssetAsync<GameObject>(tournamentReference);
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedTournamentReference = handle.Result;
                foreach (var _currentTourny in allTournaments)
                {
                    var ti= loadedTournamentReference.Clone(tournamentParent);
                    if (ti.TryGetComponent(out TournamentDisplayItem _item))
                    {
                        _item.Initialize(_currentTourny, TournamentSelected);
                    }
                }
            }else{
                Addressables.Release(handle);
            }
        }

        private void TournamentSelected(TournamentData _selectedData)
        {
            tournamentDialog.Close();
            UIUtils.OpenUI(characterSelectionData);
            TournamentController.Instance.SetSelectedTournament(_selectedData);
        }

        #endregion
        
    }
}