using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.UI.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI.DataModels
{
    public class TeamSelectionUIDataModel: MonoBehaviour
    {

       #region Nested Classes

        [Serializable]
        public class RowByClass
        {
            public CharacterClassData classData;
            public Transform rowParentTransform;
            public Image titleImage;
            public Image barImage;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Selected Team
        /// Is First time
        /// Is Random gamemode
        /// </summary>
        public static event Action<List<SavedMemberData>, bool> TeamConfirmed; 

        #endregion

        #region Serialized Fields
        
        [Header("UI Window")]

        [SerializeField] private UIWindowDialog uiWindow;

        [SerializeField] private UIWindowData matchDisplayData;
        
        [Header("Shared")]
        
        [SerializeField] private GameObject confirmButton;
        
        [Header("Team Selection - Basic")]

        [SerializeField] private AssetReference characterSelectItem;
        
        [SerializeField]
        private List<CharacterSelectDataItem> m_characterSelectDataItems = new List<CharacterSelectDataItem>();

        [SerializeField] private List<RowByClass> m_rowsByClass = new List<RowByClass>();

        [SerializeField] private RectTransform m_itemPool;
        
        #endregion

        #region Private Fields

        private List<SavedMemberData> m_selectedTeam = new List<SavedMemberData>();

        private bool m_isLoadingCharacterItem;
        
        private bool m_isLoadingSidekickItem;
        
        private bool m_isLoadingBasicItem;

        private GameObject m_loadedCaptainItemGO;
        
        private GameObject m_loadedSidekickItemGO;
        
        private GameObject m_loadedBasicItemGO;

        private GameObject m_loadedCharacterDataItemGO;
                
        private List<GameObject> m_cachedBasicItems = new List<GameObject>();
        
        private List<TeamSelectionCharacterItem> m_activeBasicItems = new List<TeamSelectionCharacterItem>();
        
        #endregion

        #region Unity Events

        private IEnumerator Start()
        {
            confirmButton.SetActive(false);

            m_characterSelectDataItems.ForEach(csdi => csdi.Initialize());
                
            if (m_loadedBasicItemGO.IsNull())
            {
                yield return StartCoroutine(C_LoadBasicCharacterItem());
            }

            RecolorAllRows();
            
            GenerateAllCharacters();
        }

        #endregion
        
        #region Class Implementation
        
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
                for (int i = 0; i < CharacterGameController.Instance.GetMaxCharacterAmount(); i++)
                {
                    var cdi= m_loadedBasicItemGO.Clone(m_itemPool);
                    cdi.SetActive(false);
                    m_cachedBasicItems.Add(cdi);
                }
            }else{
                Addressables.Release(handle);
            }

            m_isLoadingBasicItem = false;
        }

        /// <summary>
        /// Player gets to choose out of all available captains
        /// </summary>
        private void GenerateAllCharacters()
        {
            ClearSelectionItems();
            
            foreach (CharacterStatsBase _character in CharacterGameController.Instance.GetAllCharacters())
            {
                AddCharacterItem(_character, m_rowsByClass.FirstOrDefault(rbc => rbc.classData.classGUID == _character.classTyping.classGUID)?.rowParentTransform);
            }
        }

        private void RecolorAllRows()
        {
            foreach (var rowByClass in m_rowsByClass)
            {
                rowByClass.titleImage.color = rowByClass.classData.barColor;   
                rowByClass.barImage.color = rowByClass.classData.darkColor;   
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

        private void AddCharacterItem(CharacterStatsBase _character, Transform parent)
        {
            StartCoroutine(C_AddCharacterItem(_character, parent));
        }

        private IEnumerator C_AddCharacterItem(CharacterStatsBase _character, Transform parent)
        {
            if (m_loadedBasicItemGO.IsNull())
            {
                Debug.Log("<color=blue>Waiting</color>");
                yield return new WaitUntil(() => m_isLoadingBasicItem == false);
                Debug.Log("<color=blue>WAITING FINISHED</color>");
            }
            
            
            GameObject _newCharacterItem = m_cachedBasicItems.Count > 0
                ? m_cachedBasicItems.FirstOrDefault()
                : m_loadedBasicItemGO.Clone(parent);
            
           
            if (_newCharacterItem.IsNull())
            {
                Debug.Log("Character null");
            }

            if (_newCharacterItem.transform.parent != parent)
            {
                _newCharacterItem.transform.parent = parent;
            }
            
            _newCharacterItem.SetActive(true);

            if (m_cachedBasicItems.Contains(_newCharacterItem))
            {
                m_cachedBasicItems.Remove(_newCharacterItem);
            }
            
            
            _newCharacterItem.TryGetComponent(out TeamSelectionCharacterItem item);

            SavedMemberData _newMemberData = new SavedMemberData
            {
                m_characterGUID = _character.characterGUID,
                m_characterStatsBase = _character,
            };

            if (item)
            {
                item.InitializeCharacterItem(_newMemberData, OnCharacterItemPressed, UpdateHighlightItem);

                m_activeBasicItems.Add(item);
            }
        }

        private void OnCharacterItemPressed(SavedMemberData _character, TeamSelectionCharacterItem _item)
        {
            if (m_characterSelectDataItems.Count == 0)
            {
                return;
            }

            if (_character.IsNull())
            {
                return;
            }

            if (m_selectedTeam.Contains(_character))
            {
                m_selectedTeam.Remove(_character);
                _item.OnUpdateSelectedIcon(false);
                UpdateDisplays();
                return;
            }

            if (m_characterSelectDataItems.TrueForAll(cd => cd.isConfirmed))
            {
                return;
            }
            
            CharacterSelectDataItem displayToUpdate = m_characterSelectDataItems.FirstOrDefault(cd => !cd.isConfirmed);

            displayToUpdate.AssignData(_character, true);

            m_selectedTeam.Add(_character);
            _item.OnUpdateSelectedIcon(true);

            CheckTeamThreshold();
            
        }

        private void UpdateDisplays()
        {
            for (int i = 0; i < m_characterSelectDataItems.Count; i++)
            {
                if (i < m_selectedTeam.Count)
                {
                    m_characterSelectDataItems[i].AssignData(m_selectedTeam[i], true);
                }
                else
                {
                    m_characterSelectDataItems[i].ClearData();
                }
            }
        }

        private void UpdateHighlightItem(SavedMemberData _memberData, bool _isHighlight)
        {
            if (_memberData.IsNull())
            {
                return;
            }

            if (m_selectedTeam.Contains(_memberData))
            {
                return;
            }

            var displayToUpdate = m_characterSelectDataItems.FirstOrDefault(cd => !cd.isConfirmed);

            if (displayToUpdate.IsNull())
            {
                return;
            }

            if (_isHighlight)
            {
                displayToUpdate.AssignData(_memberData, false);
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
            
            confirmButton.SetActive(true);

        }

        private void CacheSelectionItem(TeamSelectionCharacterItem _item)
        {
            if (_item.IsNull())
            {
                return;
            }

            _item.CleanUpItem();
            _item.gameObject.SetActive(false);
            _item.transform.parent = m_itemPool;
            m_cachedBasicItems.Add(_item.gameObject);
        }

        public void CancelTeamConfirm()
        {
            var lastTeammate = m_selectedTeam.LastOrDefault();
            m_selectedTeam.Remove(lastTeammate);
            UpdateDisplays();
            CheckTeamThreshold();
        }

        /// <summary>
        /// Send confirmed team to team manager
        /// </summary>
        public void ConfirmTeam()
        {
            TeamConfirmed?.Invoke(m_selectedTeam, true);
            UIUtils.OpenUI(matchDisplayData);
            uiWindow.Close();
        }

        #endregion
    }
}