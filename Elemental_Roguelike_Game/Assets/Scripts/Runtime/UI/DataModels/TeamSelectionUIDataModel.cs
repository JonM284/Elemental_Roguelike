using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.UI.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.UI.DataModels
{
    public class TeamSelectionUIDataModel: MonoBehaviour
    {

        #region Actions

        /// <summary>
        /// Selected Team
        /// Is First time
        /// Is Random gamemode
        /// </summary>
        public static event Action<List<CharacterStatsBase>, bool, bool> TeamConfirmed; 

        #endregion

        #region Serialized Fields
        
        [Header("UI Window")]

        [SerializeField] private UIWindowDialog uiWindow;

        [SerializeField] private UIWindowData matchDisplayData;
        
        [Header("Team Selection - Random")]
        
        [SerializeField] private AssetReference captainUIItem;

        [SerializeField] private AssetReference sidekickUIItem;
        
        [SerializeField] private Transform randomCaptainParent;
        
        [SerializeField] private Transform randomSidekickParent;

        [SerializeField] private Transform selectedCaptainParent;

        [SerializeField] private int randomCaptainSelectAmount = 3;

        [SerializeField] private int randomSidekickGenerateAmount = 3;

        [SerializeField] private bool m_isRandomTeam;
        
        [SerializeField] private GameObject confirmButton;
        
        [Header("Team Selection - Basic")]

        [SerializeField] private AssetReference characterSelectItem;

        [SerializeField] private AssetReference characterDataDisplayItem;
        
        [SerializeField] private Transform itemParent;

        [SerializeField] private Transform characterDataParent;
        
        #endregion

        #region Private Fields

        private List<CharacterStatsBase> m_selectedTeam = new List<CharacterStatsBase>();

        private bool m_isLoadingCharacterItem;
        
        private bool m_isLoadingSidekickItem;
        
        private bool m_isLoadingBasicItem;

        private GameObject m_loadedCaptainItemGO;
        
        private GameObject m_loadedSidekickItemGO;
        
        private GameObject m_loadedBasicItemGO;

        private GameObject m_loadedCharacterDataItemGO;
        
        private List<GameObject> m_cachedCaptainItems = new List<GameObject>();

        private List<GameObject> m_cachedSidekickItems = new List<GameObject>();
        
        private List<GameObject> m_cachedBasicItems = new List<GameObject>();

        private List<TeamSelectionCharacterItem> m_activeCaptainItems = new List<TeamSelectionCharacterItem>();

        private List<TeamSelectionCharacterItem> m_activeBasicItems = new List<TeamSelectionCharacterItem>();
        
        [SerializeField]
        private List<CharacterSelectDataItem> m_characterDataItems = new List<CharacterSelectDataItem>();

        private Transform m_itemPool;

        #endregion

        #region Accessors

        private Transform itemPool => CommonUtils.GetRequiredComponent(ref m_itemPool, () =>
        {
            var t = TransformUtils.CreatePool(this.transform, false);
            return t;
        }); 

        #endregion

        #region Unity Events

        private IEnumerator Start()
        {
            confirmButton.SetActive(false);

            if (m_isRandomTeam)
            {
                if (m_loadedCaptainItemGO.IsNull())
                {
                    yield return StartCoroutine(C_LoadCaptainItem());
                }

                if (m_loadedSidekickItemGO.IsNull())
                {
                    StartCoroutine(C_LoadSidekickItem());
                }
                
                GenerateRandomCaptainSelect();

            }
            else
            {
                StartCoroutine(C_LoadCharacterDataDisplays());
                
                if (m_loadedBasicItemGO.IsNull())
                {
                    yield return StartCoroutine(C_LoadBasicCharacterItem());
                }

                GenerateAllCaptainSelect();
            }
        }

        #endregion
        
        #region Class Implementation
        
        private IEnumerator C_LoadCaptainItem()
        {
            m_isLoadingCharacterItem = true;

            var handle = Addressables.LoadAssetAsync<GameObject>(captainUIItem);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_loadedCaptainItemGO = handle.Result;
                for (int i = 0; i < 3; i++)
                {
                    var cdi= m_loadedCaptainItemGO.Clone(itemPool);
                    cdi.SetActive(false);
                    m_cachedCaptainItems.Add(cdi);
                }
            }else{
                Addressables.Release(handle);
            }

            m_isLoadingCharacterItem = false;
        }
        
        private IEnumerator C_LoadSidekickItem()
        {
            m_isLoadingSidekickItem = true;

            var handle = Addressables.LoadAssetAsync<GameObject>(sidekickUIItem);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_loadedSidekickItemGO = handle.Result;
                for (int i = 0; i < 3; i++)
                {
                    var cdi= m_loadedSidekickItemGO.Clone(itemPool);
                    cdi.SetActive(false);
                    m_cachedSidekickItems.Add(cdi);
                }
            }else{
                Addressables.Release(handle);
            }

            m_isLoadingSidekickItem = false;
        }
        
        private IEnumerator C_LoadBasicCharacterItem()
        {
            m_isLoadingBasicItem = true;

            var handle = Addressables.LoadAssetAsync<GameObject>(characterSelectItem);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_loadedBasicItemGO = handle.Result;
                for (int i = 0; i < 20; i++)
                {
                    var cdi= m_loadedBasicItemGO.Clone(itemPool);
                    cdi.SetActive(false);
                    m_cachedBasicItems.Add(cdi);
                }
            }else{
                Addressables.Release(handle);
            }

            m_isLoadingBasicItem = false;
        }

        private IEnumerator C_LoadCharacterDataDisplays()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(characterDataDisplayItem);
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                m_loadedCharacterDataItemGO = handle.Result;
                for (int i = 0; i < 4; i++)
                {
                    var csdi= m_loadedCharacterDataItemGO.Clone(characterDataParent);
                    if (csdi.TryGetComponent(out CharacterSelectDataItem _item))
                    {
                        bool _isCaptain = i == 0;
                        Action<CharacterStatsBase> callback = _isCaptain ? OnDeleteCaptainData : OnDeleteSidekickData;
                        _item.Initialize(_isCaptain, callback);
                        m_characterDataItems.Add(_item);
                    }
                }
            }else{
                Addressables.Release(handle);
            }
        }

        /// <summary>
        /// Random Captains are picked for the beginning of a run, 
        /// </summary>
        public void GenerateRandomCaptainSelect()
        {
            var randomCaptains = CharacterGameController.Instance.GetRandomCaptains(randomCaptainSelectAmount);
            
            foreach (var captain in randomCaptains)
            {
                AddCharacterItem(captain, true, randomCaptainParent);
            }
        }

        /// <summary>
        /// Random sidekicks are chosen for the player, they only need to be able to see what abilities and stats each sidekick has
        /// </summary>
        private void GenerateRandomSidekicksDisplay()
        {
            var randomSidekicks = CharacterGameController.Instance.GetRandomSidekicks(randomSidekickGenerateAmount);
            
            foreach (var sidekick in randomSidekicks)
            {
                AddCharacterItem(sidekick, false, randomSidekickParent);
                if (!m_selectedTeam.Contains(sidekick))
                {
                    m_selectedTeam.Add(sidekick);
                }
            }
            
            confirmButton.SetActive(true);
        }

        /// <summary>
        /// Player gets to choose out of all available captains
        /// </summary>
        private void GenerateAllCaptainSelect()
        {
            ClearSelectionItems();
            
            var allCaptains = CharacterGameController.Instance.GetALLCaptains();
            
            foreach (var captain in allCaptains)
            {
                AddCharacterItem(captain, true, itemParent);
            }
        }

        /// <summary>
        /// Player gets to choose out of all availabale sidekicks
        /// </summary>
        private void GenerateAllSidekickSelect()
        {
            ClearSelectionItems();
            
            Debug.Log("GENERATING all sidekicks");
            
            var allSidekicks = CharacterGameController.Instance.GetALLSidekicks();

            foreach (var sidekick in allSidekicks)
            {
                AddCharacterItem(sidekick, false, itemParent);
            }
        }

        private void ClearSelectionItems()
        {
            if (m_activeBasicItems.Count == 0)
            {
                return;
            }
            
            m_activeBasicItems.ForEach(CacheSelectionItem);
            
            m_activeBasicItems.Clear();
        }

        private void PressedCallback(CharacterStatsBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            if (!m_selectedTeam.Contains(_character))
            {
                m_selectedTeam.Add(_character);
            }
            
            m_activeCaptainItems.ForEach(tsci =>
            {
                if (tsci.m_assignedStats != _character)
                {
                    CacheCaptain(tsci);
                }
                else
                {
                    tsci.transform.parent = selectedCaptainParent;
                    tsci.gameObject.TryGetComponent(out RectTransform _rectTransform);
                    if (_rectTransform)
                    {
                        _rectTransform.anchoredPosition = Vector2.zero;
                    }
                }
            });

            if (m_isRandomTeam)
            {
                GenerateRandomSidekicksDisplay();
            }
            else
            {
                GenerateAllSidekickSelect();
            }
            
            Debug.Log("Clicked");
        }

        private void AddCharacterItem(CharacterStatsBase _character, bool _isCaptain, Transform parent)
        {
            StartCoroutine(C_AddCharacterItem(_character, _isCaptain, parent));
        }

        private IEnumerator C_AddCharacterItem(CharacterStatsBase _character, bool _isCaptain, Transform parent)
        {
            if (m_isRandomTeam)
            {
                if (_isCaptain)
                {
                    if (m_loadedCaptainItemGO.IsNull())
                    {
                        Debug.Log("<color=blue>Waiting</color>");
                        yield return new WaitUntil(() => m_isLoadingCharacterItem == false);
                        Debug.Log("<color=blue>WAITING FINISHED</color>");
                    }
                }
                else
                {
                    if (m_loadedSidekickItemGO.IsNull())
                    {
                        Debug.Log("<color=blue>Waiting</color>");
                        yield return new WaitUntil(() => m_isLoadingSidekickItem == false);
                        Debug.Log("<color=blue>WAITING FINISHED</color>");
                    }
                }    
            }
            else
            {
                if (m_loadedBasicItemGO.IsNull())
                {
                    Debug.Log("<color=blue>Waiting</color>");
                    yield return new WaitUntil(() => m_isLoadingBasicItem == false);
                    Debug.Log("<color=blue>WAITING FINISHED</color>");
                }
            }
            
            
            GameObject newCharacter;

            if (m_isRandomTeam)
            {
                if (_isCaptain)
                {
                    newCharacter = m_cachedCaptainItems.Count > 0 
                        ? m_cachedCaptainItems.FirstOrDefault()
                        : m_loadedCaptainItemGO.Clone(parent);
                }
                else
                {
                    newCharacter = m_cachedSidekickItems.Count > 0
                        ? m_cachedSidekickItems.FirstOrDefault()
                        : m_loadedSidekickItemGO.Clone(parent);
                }
            }
            else
            {
                newCharacter = m_cachedBasicItems.Count > 0
                    ? m_cachedBasicItems.FirstOrDefault()
                    : m_loadedBasicItemGO.Clone(parent);
            }
            
           
            if (newCharacter.IsNull())
            {
                Debug.Log("Character null");
            }

            if (newCharacter.transform.parent != parent)
            {
                newCharacter.transform.parent = parent;
            }
            
            newCharacter.SetActive(true);

            if (m_isRandomTeam)
            {
                if (_isCaptain)
                {
                    if (m_cachedCaptainItems.Contains(newCharacter))
                    {
                        m_cachedCaptainItems.Remove(newCharacter);
                    }    
                }
                else
                {
                    if (m_cachedSidekickItems.Contains(newCharacter))
                    {
                        m_cachedSidekickItems.Remove(newCharacter);
                    }
                }
            }
            else
            {
                if (m_cachedBasicItems.Contains(newCharacter))
                {
                    m_cachedBasicItems.Remove(newCharacter);
                }
            }
            
            
            newCharacter.TryGetComponent(out TeamSelectionCharacterItem item);
            if (item)
            {
                if (m_isRandomTeam)
                {
                    Action<CharacterStatsBase> _pressedAction = _isCaptain ? PressedCallback : null;
                    Action<CharacterStatsBase, bool> _highlightAction = null;
                    item.InitializeCharacterItem(_character, _pressedAction, _highlightAction);
                
                    if (_isCaptain)
                    {
                        m_activeCaptainItems.Add(item);
                    }    
                }
                else
                {
                    Action<CharacterStatsBase> _pressedAction = _isCaptain ? UpdateCaptainData : UpdateSidekickData;
                    Action<CharacterStatsBase, bool> _highlightAction = UpdateHighlightItem;
                    item.InitializeCharacterItem(_character, _pressedAction, _highlightAction);

                    m_activeBasicItems.Add(item);
                }
                
            }
        }

        private void OnDeleteCaptainData(CharacterStatsBase _character)
        {
            if (m_selectedTeam.Contains(_character))
            {
                m_selectedTeam.Remove(_character);
            }
            
            CheckTeamThreshold();
            GenerateAllCaptainSelect();
        }

        private void OnDeleteSidekickData(CharacterStatsBase _character)
        {
            if (m_selectedTeam.Contains(_character))
            {
                m_selectedTeam.Remove(_character);
            }
            
            CheckTeamThreshold();
            
            //check what is currently shown

            if (m_activeBasicItems.Count == 0)
            {
                GenerateAllSidekickSelect();
            }
            
        }

        private void UpdateCaptainData(CharacterStatsBase _character)
        {
            if (m_characterDataItems.Count == 0)
            {
                return;
            }

            if (_character.IsNull())
            {
                return;
            }
            
            m_characterDataItems[0].AssignData(_character, true);

            m_selectedTeam.Add(_character);

            CheckTeamThreshold();   
                
            if (m_selectedTeam.Count < TeamController.Instance.teamSize)
            {
                GenerateAllSidekickSelect();
            }
        }

        private void UpdateSidekickData(CharacterStatsBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            var displayToUpdate = m_characterDataItems.FirstOrDefault(cd => !cd.isConfirmed);

            if (displayToUpdate.IsNull())
            {
                Debug.LogError("CANT FIND UNASSIGNED SIDEKICK");
                return;
            }

            displayToUpdate.AssignData(_character, true);

            m_selectedTeam.Add(_character);

            CheckTeamThreshold();
        }

        private void UpdateHighlightItem(CharacterStatsBase _character, bool _isHighlight)
        {
            if (_character.IsNull())
            {
                return;
            }

            var displayToUpdate = m_characterDataItems.FirstOrDefault(cd => !cd.isConfirmed);

            if (displayToUpdate.IsNull())
            {
                return;
            }

            if (_isHighlight)
            {
                displayToUpdate.AssignData(_character, false);
            }
            else
            {
                displayToUpdate.ClearData();
            }
        }

        private void CheckTeamThreshold()
        {
            if (m_selectedTeam.Count < TeamController.Instance.teamSize)
            {
                confirmButton.SetActive(false);
                return;
            }

            ClearSelectionItems();
            
            confirmButton.SetActive(true);

        }
        
        private void CacheCaptain(TeamSelectionCharacterItem _item)
        {
            if (_item.IsNull())
            {
                return;
            }
            
            _item.CleanUpItem();
            _item.gameObject.SetActive(false);
            _item.transform.parent = itemPool;
        }

        private void CacheSelectionItem(TeamSelectionCharacterItem _item)
        {
            if (_item.IsNull())
            {
                return;
            }

            _item.CleanUpItem();
            _item.gameObject.SetActive(false);
            _item.transform.parent = itemPool;
            m_cachedBasicItems.Add(_item.gameObject);
        }

        /// <summary>
        /// Send confirmed team to team manager
        /// </summary>
        public void ConfirmTeam()
        {
            TeamConfirmed?.Invoke(m_selectedTeam, true, m_isRandomTeam);
            UIUtils.OpenUI(matchDisplayData);
            uiWindow.Close();
        }

        #endregion
    }
}