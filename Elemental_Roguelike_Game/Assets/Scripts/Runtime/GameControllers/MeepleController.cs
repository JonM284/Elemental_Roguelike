using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.DataSaving;
using Project.Scripts.Data;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class MeepleController: GameControllerBase, ISaveableData
    {

        #region Events

        public static event Action<CharacterBase, CharacterStatsData> PlayerMeepleCreated;
        
        #endregion

        #region Serialized Fields

        [SerializeField] private AssetReference meepleAsset;

        [SerializeField] private AssetReference enemyMeeplAsset;

        [SerializeField] private bool randomizeCharacter;

        #endregion
        
        #region Private Fields

        //Saved data of all owned meeples
        private SerializableDictionary<string, CharacterStatsData> m_allOwnedCharacters;

        //Dictionary to keep track of meeple gameobjects that were already created. Might not be used
        private Dictionary<string, CharacterBase> m_cachedMeepleCharacters = new Dictionary<string, CharacterBase>(); 

        #endregion

        #region Unity Events

        private void OnEnable()
        {
            LevelGenerationManager.LevelGenerationFinished += PlaceStartCharacter;
        }

        private void OnDisable()
        {
            LevelGenerationManager.LevelGenerationFinished -= PlaceStartCharacter;
        }

        #endregion
        
        #region Class Implementation

        private void PlaceStartCharacter(RoomTracker _roomTracker)
        {
            if (meepleAsset == null)
            {
                Debug.LogError("Meeple asset not assigned");
                return;
            }

            if (m_allOwnedCharacters.Count == 0)
            {
                TestCreateNewCharacter();
            }

            var firstCharacter = m_allOwnedCharacters.FirstOrDefault();
            InstantiatePremadeMeeple(firstCharacter);
        }

        public void DeletePlayerMeeple(string _meepleGUID)
        {
            if (_meepleGUID == "")
            {
                return;
            }

            if (!m_allOwnedCharacters.ContainsKey(_meepleGUID))
            {
                Debug.LogError("Deleted meeple doesn't exist");
                return;
            }

            m_allOwnedCharacters.Remove(_meepleGUID);
        }

        [ContextMenu("Clear all characters")]
        public void ClearAllCharacters()
        {
            m_allOwnedCharacters.Clear();
        }
        
        [ContextMenu("Test Create Character")]
        public void TestCreateNewCharacter()
        {
            var newCharacter = new CharacterStatsData();
            if (randomizeCharacter)
            {
                RandomizeCharacterVariables(newCharacter);
            }
            newCharacter.id = System.Guid.NewGuid().ToString();
            m_allOwnedCharacters.Add(newCharacter.id, newCharacter);
        }

        private void RandomizeCharacterVariables(CharacterStatsData _character)
        {
            _character.type = ElementUtils.GetRandomElement();
            _character.initiativeNumber = Random.Range(1, 20);
            _character.weapon = Random.Range(0, 10);
            _character.baseDamage = Random.Range(1f, 10f);
            _character.baseHealth = Random.Range(10, 20);
            _character.baseShields = Random.Range(10, 20);
            _character.baseSpeed = Random.Range(1, 10);
            _character.movementDistance = Random.Range(5, 10);
            for (int i = 0; i < 2; i++)
            {
                _character.abilityReferences.Add(AbilityUtils.GetRandomAbilityByType(_character.type).abilityGUID);
            }
        }

        public void InstantiatePremadeMeeple(KeyValuePair<string,CharacterStatsData> _meepleCharacter)
        {
            //instantiate gameobject
            var handle = Addressables.LoadAssetAsync<GameObject>(meepleAsset);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newMeepleObject = Instantiate(handle.Result);
                    newMeepleObject.transform.position = new Vector3(0,newMeepleObject.transform.localScale.y / 2,0);
                    var newMeeple = newMeepleObject.GetComponent<CharacterBase>();
                    if (newMeeple != null)
                    {
                        if (newMeeple is PlayableCharacter playableCharacter)
                        {
                            playableCharacter.AssignStats(_meepleCharacter.Value);
                        }
                        newMeeple.InitializeCharacter();
                        PlayerMeepleCreated?.Invoke(newMeeple, _meepleCharacter.Value);
                    }
                }
            };
        }

        public void InstantiateMeepleEnemy(CharacterStatsData _meepleStats)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(enemyMeeplAsset);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newMeepleObject = Instantiate(handle.Result);
                    newMeepleObject.transform.position = new Vector3(0,newMeepleObject.transform.localScale.y / 2,0);
                    var newMeeple = newMeepleObject.GetComponent<CharacterBase>();
                    if (newMeeple != null)
                    {
                        if (newMeeple is EnemyCharacterMeeple enemyMeeple)
                        {
                            enemyMeeple.AssignStats(_meepleStats);
                        }
                        newMeeple.InitializeCharacter();
                    }
                }
            };
        }

        public CharacterStatsData GetMeepleFromUID(string _UID)
        {
            return m_allOwnedCharacters.FirstOrDefault(c => c.Key == _UID).Value;
        }

        #endregion

        
        #region ISaveableData Methods

        public void LoadData(SavedGameData _savedGameData)
        {
            m_allOwnedCharacters = _savedGameData.allOwnedCharacters;
        }

        public void SaveData(ref SavedGameData _savedGameData)
        {
            _savedGameData.allOwnedCharacters = m_allOwnedCharacters;
        }

        #endregion
        
    }
}