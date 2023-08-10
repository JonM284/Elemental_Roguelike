using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Cards;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.UI;
using Runtime.UI.DataModels;
using Runtime.UI.DataReceivers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.Submodules
{
    public class RandomTeamSelectionManager : MonoBehaviour
    {

        #region Nested Classes 

        [Serializable]
        public class CardByClass
        {
            public CharacterClassData classData;
            public AssetReference cardRef;
            public GameObject loadedCardGO;
        }

        #endregion

        #region Actions

        public static event Action<List<CharacterStatsData>> GeneratedTeamData; 

        public static event Action<CharacterStatsData> SelectedMeepleConfirmed;
        
        public static event Action<List<CharacterStatsData>> TeamMembersConfirmed;

        #endregion

        #region Events

        public UnityEvent onTeamConfirmed;

        public UnityEvent onFinishedLoading;

        public UnityEvent onTeamAmountReached;

        public UnityEvent onTeamAmountReduced;

        #endregion
        
        #region Serialized Fields

        [SerializeField] private List<CardByClass> cardsByClass = new List<CardByClass>();

        [SerializeField] private AssetReference displayMeepleRef;

        [SerializeField] private UIWindowData uiWindowData;

        [SerializeField] private List<Transform> generatedCardLocations = new List<Transform>();

        [SerializeField] private List<Transform> selectedTeamLocations = new List<Transform>();

        [SerializeField] private Transform cardSpawnLocation;
        
        #endregion

        #region Private Fields

        private GameObject loadedDisplayMeepleObject;

        private List<MeepleCardItem> m_cachedCards = new List<MeepleCardItem>();

        private List<MeepleCardItem> m_activeCards = new List<MeepleCardItem>();

        private List<MeepleCardItem> m_selectedCards = new List<MeepleCardItem>();
        
        private List<MeepleCardItem> m_randomGeneratedCards = new List<MeepleCardItem>();

        private List<GameObject> m_cachedSavedMeepleObjects = new List<GameObject>();
        
        private MeepleController m_meepleController;

        private TeamController m_teamController;

        private List<CharacterStatsData> randomGeneratedTeamData = new List<CharacterStatsData>();

        private List<CharacterStatsData> selectedTeamData = new List<CharacterStatsData>();

        private Transform m_MeepleObjPool;

        private Transform m_cardObjPool;

        private bool m_isButtonOpen;
        
        #endregion

        #region Accessors

        private MeepleController meepleController => GameControllerUtils.GetGameController(ref m_meepleController);
        
        private TeamController teamController => GameControllerUtils.GetGameController(ref m_teamController);

        private Transform cachedMeepleObjectPool => CommonUtils.GetRequiredComponent(ref m_MeepleObjPool, () =>
        {
            var meeplePool = TransformUtils.CreatePool(this.transform, false);
            return meeplePool;
        });
        
        private Transform cachedCardObjectPool => CommonUtils.GetRequiredComponent(ref m_cardObjPool, () =>
        {
            var cardPool = TransformUtils.CreatePool(this.transform, false);
            return cardPool;
        });

        private int generatedTeamSize => teamController.generatedTeamSize;
        

        #endregion

        #region Unity Events
        
        private void OnEnable()
        {
            MeepleCardItem.MeepleItemSelected += OnMeepleItemSelected;
            MeepleTeamSelectionDataModel.RerollRequested += GenerateNewTeam;
        }

        private void OnDisable()
        {
            MeepleCardItem.MeepleItemSelected -= OnMeepleItemSelected;
            MeepleTeamSelectionDataModel.RerollRequested -= GenerateNewTeam;
        }

        #endregion

        #region Class Implementation

        public void ConfirmTeam()
        {
            if (selectedTeamData.Count != 5)
            {
                return;
            }

            TeamMembersConfirmed?.Invoke(selectedTeamData);

            onTeamConfirmed?.Invoke();
            
            m_selectedCards.ForEach(mci => CacheCard(mci));
            
            m_randomGeneratedCards.ForEach(mci => CacheCard(mci));
            
            m_selectedCards.Clear();
            m_randomGeneratedCards.Clear();
        }


        public void SetupRandomTeamGenerator()
        {
            StartCoroutine(C_SetupRandomGenerator());
        }
        
        private IEnumerator C_SetupRandomGenerator()
        {

            yield return C_LoadMeeple();

            foreach (var cardByClass in cardsByClass)
            {
                yield return C_LoadCardGameObject(cardByClass);
            }

            yield return C_PreInstantiateGameObjects();
            
            onFinishedLoading?.Invoke();

            yield return new WaitForSeconds(0.3f);
            
            GenerateNewTeam();

        }

        private IEnumerator C_PreInstantiateGameObjects()
        {
            for (int i = 0; i < 8; i++)
            {
                var instantiatedMeeple = Instantiate(loadedDisplayMeepleObject, cachedMeepleObjectPool);
                m_cachedSavedMeepleObjects.Add(instantiatedMeeple);
            }

            foreach (var cardByClass in cardsByClass)
            {
                for (int i = 0; i < 8; i++)
                {
                    var instantiatedCard = Instantiate(cardByClass.loadedCardGO, cachedCardObjectPool);
                    instantiatedCard.TryGetComponent(out MeepleCardItem cardItem);
                    if (!cardItem.IsNull())
                    {
                        CacheCard(cardItem);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);

        }

        private IEnumerator C_LoadMeeple()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(displayMeepleRef);
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var newMeepleObject = Instantiate(handle.Result, cachedMeepleObjectPool);
                loadedDisplayMeepleObject = handle.Result;
                newMeepleObject.transform.localPosition = Vector3.zero;
            }
        }

        private GameObject GetLoadedCardObject(CardByClass cardByClass)
        {
            if (cardByClass.IsNull())
            {
                return default;
            }

            return cardByClass.loadedCardGO;
        }

        private IEnumerator C_LoadCardGameObject(CardByClass cardByClass)
        {
            var handle = cardByClass.cardRef.LoadAssetAsync<GameObject>();
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                cardByClass.loadedCardGO = handle.Result;
            }else {
                Addressables.Release(handle);
            }
        }

        public IEnumerator C_InstantiateCard(CharacterStatsData _data , CardByClass _cardByClass)
        {
            var loadedObj = GetLoadedCardObject(_cardByClass);
            if (loadedObj.IsNull())
            {
                yield return C_LoadCardGameObject(_cardByClass);
            }

            Instantiate(loadedObj);
            loadedObj.TryGetComponent(out MeepleCardItem cardItem);
            if (cardItem)
            {
                cardItem.InitializeSelectionItem(_data);
            }
        }

        private void Release()
        {
            foreach (var cardByClass in cardsByClass)
            {
                cardByClass.cardRef.ReleaseAsset();
            }
            
            displayMeepleRef.ReleaseAsset();
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
            StartCoroutine(C_GenerateNewTeam());
        }
        
        public IEnumerator C_GenerateNewTeam()
        {

            if (randomGeneratedTeamData.Count > 0)
            {
                if (m_randomGeneratedCards.Count > 0)
                {
                    foreach (var cardObj in m_randomGeneratedCards)
                    {
                        if (!cardObj.assignedMeepleObj.IsNull())
                        {
                            CacheMeepleGameObject(cardObj.assignedMeepleObj);
                            cardObj.CleanUp();
                            CacheCard(cardObj);
                        }
                    }
                    
                    m_randomGeneratedCards.Clear();
                }
                randomGeneratedTeamData.Clear();
            }
            
            for (int i = 0; i < generatedTeamSize; i++)
            {
                var newCharacter = meepleController.CreateNewCharacter();
                
                randomGeneratedTeamData.Add(newCharacter);

                var _foundClass = meepleController.GetClassByGUID(newCharacter.classReferenceType);
                
                //Get Card
                var card = m_cachedCards.FirstOrDefault(card => card.classData == _foundClass);

                m_cachedCards.Remove(card);
                card.transform.parent = null;
                card.transform.position = cardSpawnLocation.position;
                
                card.SetMovement(generatedCardLocations[i].position, generatedCardLocations[i].forward, true);
                
                m_activeCards.Add(card);
                m_randomGeneratedCards.Add(card);
                
                card.InitializeSelectionItem(newCharacter);
                
                InstantiateDisplayMeeple(newCharacter, card);
                
                //ToDo: Add to Movement list, to move card
                
                
                yield return null;
            }
            
            GeneratedTeamData?.Invoke(randomGeneratedTeamData);
        }

        private void InstantiateDisplayMeeple(CharacterStatsData data, MeepleCardItem cardItem)
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

                meepleGO.transform.parent = cardItem.displayMeepleLocation;
                meepleGO.transform.localPosition = Vector3.zero;
                meepleGO.transform.forward = cardItem.displayMeepleLocation.forward;

                meepleGO.TryGetComponent(out DisplayMeeple _displayMeeple);
                if (_displayMeeple)
                {
                    //Add to current Display
                    _displayMeeple.InitializeDisplay(data);
                }
                
                cardItem.AssignDisplayMeeple(meepleGO);
                
                return;
            }
            
            
            //If there is no usable Meeple gameObject, create a new one
            var clonedMeepleObj = Instantiate(loadedDisplayMeepleObject, cardItem.displayMeepleLocation);
            clonedMeepleObj.transform.localPosition = Vector3.zero;
            
            clonedMeepleObj.TryGetComponent(out DisplayMeeple _dispMeeple);
            
            //Not exactly necessary, useful for debugging
            if (_dispMeeple.IsNull())
            {
                Debug.LogError("Display meeple instantiated but doesn't have display meeple component");
                return;
            }

            _dispMeeple.InitializeDisplay(data);
            cardItem.AssignDisplayMeeple(clonedMeepleObj);
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
                    UpdateSelectedListLocations();
                    return;
                }
            }
            
            //Don't add more than 5
            if (selectedTeamData.Count == 5)
            {
                return;
            }

            var foundCard = m_randomGeneratedCards.FirstOrDefault(cardItem => cardItem.assignedData.id == _selectedData.id);

            if (foundCard.IsNull())
            {
                Debug.LogError("Meeple not found");
                return;
            }

            //Remove Meeple from RANDOM pool
            m_randomGeneratedCards.Remove(foundCard);

            //Enter Meeple into SELECTED TEAM pool
            m_selectedCards.Add(foundCard);
            
            //set selected
            var selectedMeeple = randomGeneratedTeamData.FirstOrDefault(ml => ml.id == _selectedData.id);
            
            randomGeneratedTeamData.Remove(selectedMeeple);
            selectedTeamData.Add(selectedMeeple);

            UpdateSelectedListLocations();
        }

        private void UpdateSelectedListLocations()
        {
            for (int i = 0; i < m_selectedCards.Count; i++)
            {
                m_selectedCards[i].SetMovement(selectedTeamLocations[i].position, selectedTeamLocations[i].forward, false);
            }

            if (m_selectedCards.Count == 5)
            {
                onTeamAmountReached?.Invoke();
                m_isButtonOpen = true;
            }
            else
            {
                if (m_isButtonOpen)
                {
                    onTeamAmountReduced?.Invoke();
                    m_isButtonOpen = false;
                }
            }
            
        }

        //Get rid of stats, put UI back into usable list
        private void DeleteSelectedMeeple(CharacterStatsData _dataToDelete)
        {

            var deletingCard =
                m_selectedCards.FirstOrDefault(cardItem => cardItem.assignedData.id == _dataToDelete.id);

            CacheMeepleGameObject(deletingCard.assignedMeepleObj);
            
            m_selectedCards.Remove(deletingCard);
            
            selectedTeamData.Remove(_dataToDelete);
            
            //unselect Item
            CacheCard(deletingCard);

        }

        private void CacheMeepleGameObject(GameObject meeple)
        {
            //Cache Obj
            m_cachedSavedMeepleObjects.Add(meeple);
                        
            //Hide Meeple Obj
            meeple.transform.parent = cachedMeepleObjectPool;
        }

        private void CacheCard(MeepleCardItem _item)
        {
            if (_item.IsNull())
            {
                return;
            }
            
            m_activeCards.Remove(_item);
            m_cachedCards.Add(_item);

            _item.transform.parent = cachedCardObjectPool;
        }

        #endregion


    }
}
