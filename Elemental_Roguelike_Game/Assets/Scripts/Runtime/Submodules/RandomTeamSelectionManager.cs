using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.UI;
using Runtime.UI.DataReceivers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.Submodules
{
    public class RandomTeamSelectionManager : MonoBehaviour
    {

        #region Nested Classes

        [Serializable]
        public class MeepleLocation
        {
            public Transform randomGeneratedMeepleLocation;
            public CharacterStatsData assignedData;
            public bool isSelected;
        }

        #endregion

        #region Actions

        public static event Action<List<CharacterStatsData>> GeneratedTeamData; 

        public static event Action<CharacterStatsData> SelectedMeepleConfirmed;

        #endregion
        
        #region Serialized Fields
        
        [SerializeField] private List<MeepleLocation> allMeepleLocations = new List<MeepleLocation>();
        
        [SerializeField] private AssetReference displayMeepleRef;

        [SerializeField] private UIWindowData uiWindowData;
        
        #endregion

        #region Private Fields

        private GameObject loadedDisplayMeepleObject;

        private List<DisplayMeeple> m_currentDisplaySelectedMeepleObjs = new List<DisplayMeeple>();

        private List<DisplayMeeple> m_currentDisplayRandomMeepleObj = new List<DisplayMeeple>();

        private List<GameObject> m_cachedSavedMeepleObjects = new List<GameObject>();

        private List<MeepleLocation> m_usableMeepleLocations = new List<MeepleLocation>();

        private MeepleController m_meepleController;

        private TeamController m_teamController;

        private List<CharacterStatsData> randomGeneratedTeamData = new List<CharacterStatsData>();

        private List<CharacterStatsData> selectedTeamData = new List<CharacterStatsData>();

        private Transform m_MeepleObjPool;

        #endregion

        #region Accessors

        private MeepleController meepleController => GameControllerUtils.GetGameController(ref m_meepleController);
        
        private TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);

        private Transform cachedMeepleObjectPool => CommonUtils.GetRequiredComponent(ref m_MeepleObjPool, () =>
        {
            var meeplePool = TransformUtils.CreatePool(this.transform, false);
            return meeplePool;
        });

        private int generatedTeamSize => teamController.generatedTeamSize;

        private int currentSelectedTeamSize => m_currentDisplaySelectedMeepleObjs.Count - 1;
        

        #endregion

        #region Unity Events

        private void Start()
        {
            SetupRandomGenerator();
            OpenUI();
        }

        //ToDo: Add Events to buttons to add and subtract from teams
        private void OnEnable()
        {
            MeepleManagerSelectionItem.MeepleItemSelected += OnMeepleItemSelected;
            MeepleTeamSelectionDataModel.RerollRequested += GenerateNewTeam;
        }

        private void OnDisable()
        {
            MeepleManagerSelectionItem.MeepleItemSelected -= OnMeepleItemSelected;
            MeepleTeamSelectionDataModel.RerollRequested -= GenerateNewTeam;
        }

        #endregion

        #region Class Implementation

        private void SetupRandomGenerator()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(displayMeepleRef);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newMeepleObject = Instantiate(handle.Result, cachedMeepleObjectPool);
                    loadedDisplayMeepleObject = handle.Result;
                    newMeepleObject.transform.localPosition = Vector3.zero;
                }
            };
        }

        private void OpenUI()
        {
            UIUtils.OpenUI(uiWindowData);
        }


        /// <summary>
        /// Create a new team, when player gets to this point, and if they chose to regenerate a new team
        /// </summary>
        [ContextMenu("Generate Team")]
        public void GenerateNewTeam()
        {

            if (randomGeneratedTeamData.Count > 0)
            {
                if (m_currentDisplayRandomMeepleObj.Count > 0)
                {
                    foreach (var randomMeepleObj in m_currentDisplayRandomMeepleObj)
                    {
                        CacheMeepleGameObject(randomMeepleObj.gameObject);
                    }
                    
                    m_currentDisplayRandomMeepleObj.Clear();
                    
                }
                randomGeneratedTeamData.Clear();
            }
            
            m_usableMeepleLocations.Clear();

            allMeepleLocations.ForEach(ml =>
            {
                if (!ml.isSelected)
                {
                    m_usableMeepleLocations.Add(ml);
                }
            });
            

            for (int i = 0; i < generatedTeamSize; i++)
            {
                var newCharacter = meepleController.CreateNewCharacter();
                
                randomGeneratedTeamData.Add(newCharacter);
                
                m_usableMeepleLocations[i].assignedData = newCharacter;
                
                InstantiateDisplayMeeple(newCharacter, m_usableMeepleLocations[i].randomGeneratedMeepleLocation);
            }
            
            GeneratedTeamData?.Invoke(randomGeneratedTeamData);

        }

        private void InstantiateDisplayMeeple(CharacterStatsData data, Transform spawnTransform)
        {
            if (loadedDisplayMeepleObject.IsNull())
            {
                return;
            }

            //If there are already usable gameObject, just re-initialize and display them
            if (m_cachedSavedMeepleObjects.Count > 0)
            {
                //Remove First in list
                var meepleGO = m_cachedSavedMeepleObjects.FirstOrDefault();
                m_cachedSavedMeepleObjects.Remove(meepleGO);

                meepleGO.transform.parent = spawnTransform;
                meepleGO.transform.localPosition = Vector3.zero;

                meepleGO.TryGetComponent(out DisplayMeeple _displayMeeple);
                if (_displayMeeple)
                {
                    //Add to current Display
                    m_currentDisplayRandomMeepleObj.Add(_displayMeeple);
                    _displayMeeple.InitializeDisplay(data);
                }
                
                
                return;
            }
            
            
            //If there is no usable Meeple gameObject, create a new one
            var clonedMeepleObj = Instantiate(loadedDisplayMeepleObject, spawnTransform);
            clonedMeepleObj.transform.localPosition = Vector3.zero;
            
            clonedMeepleObj.TryGetComponent(out DisplayMeeple _dispMeeple);

            m_currentDisplayRandomMeepleObj.Add(_dispMeeple);
            
            //Not exactly necessary, useful for debugging
            if (_dispMeeple.IsNull())
            {
                Debug.LogError("Display meeple instantiated but doesn't have display meeple component");
                return;
            }

            _dispMeeple.InitializeDisplay(data);
            
            
        }
        
        private void OnMeepleItemSelected(CharacterStatsData _selectedData)
        {
            if (_selectedData.IsNull())
            {
                return;
            }

            //1. Check if the button selected is already in selected team
            if (selectedTeamData.Count > 0)
            {
                if (selectedTeamData.Contains(_selectedData))
                {
                    //ToDo: confirm dialog
                    DeleteSelectedMeeple(_selectedData);
                    SelectedMeepleConfirmed?.Invoke(_selectedData);
                    return;
                }
            }

            var foundMeeple = randomGeneratedTeamData.FirstOrDefault(cs => cs.id == _selectedData.id);

            if (foundMeeple.IsNull())
            {
                Debug.LogError("Meeple not found");
                return;
            }

            var selectedRandomMeeple =
                m_currentDisplayRandomMeepleObj.FirstOrDefault(dm => dm.assignedData.id == foundMeeple.id);

            //Remove Meeple from RANDOM pool
            m_currentDisplayRandomMeepleObj.Remove(selectedRandomMeeple);

            //Enter Meeple into SELECTED TEAM pool
            m_currentDisplaySelectedMeepleObjs.Add(selectedRandomMeeple);
            
            //set selected
            var selectedMeeple = allMeepleLocations.FirstOrDefault(ml => ml.assignedData.id == _selectedData.id);
            if (!selectedMeeple.IsNull())
            {
                selectedMeeple.isSelected = true;
            }
            
            randomGeneratedTeamData.Remove(foundMeeple);
            selectedTeamData.Add(foundMeeple);
            
            SelectedMeepleConfirmed?.Invoke(_selectedData);
        }

        //Get rid of stats, put UI back into usable list
        private void DeleteSelectedMeeple(CharacterStatsData _dataToDelete)
        {

            var deletingMeeple =
                m_currentDisplaySelectedMeepleObjs.FirstOrDefault(dm => dm.assignedData.id == _dataToDelete.id);

            CacheMeepleGameObject(deletingMeeple.gameObject);
            
            m_currentDisplaySelectedMeepleObjs.Remove(deletingMeeple);
            
            selectedTeamData.Remove(_dataToDelete);
            
            //unselect Item
            var deselectedMeeple = allMeepleLocations.FirstOrDefault(ml => ml.assignedData.id == _dataToDelete.id);

            deselectedMeeple.isSelected = false;

            deselectedMeeple.assignedData = new CharacterStatsData();

        }

        private void CacheMeepleGameObject(GameObject meeple)
        {
            //Cache Obj
            m_cachedSavedMeepleObjects.Add(meeple);
                        
            //Hide Meeple Obj
            meeple.transform.parent = cachedMeepleObjectPool;
        }

        #endregion


    }
}
