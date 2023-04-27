using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.DataSaving;
using Project.Scripts.Data;
using Project.Scripts.Runtime.LevelGeneration;
using Project.Scripts.Utils;
using Runtime.Abilities;
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

        private List<GameObject> m_cachedMeepleObjs = new List<GameObject>();

        private GameObject loadedPlayableMeeple;

        private GameObject loadedEnemyMeeple;

        private Transform m_cachedMeeplePoolTransform;

        #endregion

        #region Accessors

        public Transform cachedMeepleObjPool =>
            CommonUtils.GetRequiredComponent(ref m_cachedMeeplePoolTransform, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                return poolTransform;
            });

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
                CreateNewCharacter();
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
        
        [ContextMenu("Create Character")]
        public void CreateNewCharacter()
        {
            if (m_allOwnedCharacters.Count > 10)
            {
                return;
            }
            
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
            _character.meepleElementTypeRef = ElementUtils.GetRandomElement().elementGUID;
            _character.initiativeNumber = Random.Range(1, 20);
            //TODO:assign default weapon ******
            _character.baseDamage = Random.Range(1f, 10f);
            _character.baseHealth = Random.Range(10, 20);
            _character.baseShields = Random.Range(10, 20);
            _character.baseSpeed = Random.Range(1, 10);
            _character.movementDistance = Random.Range(5, 10);
            for (int i = 0; i < 2; i++)
            {
                var randomAbility = AbilityUtils.GetRandomAbilityByType(_character.meepleElementTypeRef);
                _character.abilityReferences.Add(randomAbility.abilityGUID);
            }

            _character.weaponReference = WeaponUtils.GetDefaultWeapon().weaponGUID;
            _character.weaponElementTypeRef = ElementUtils.GetDefault().elementGUID;
        }

        public void InstantiatePremadeMeeple(KeyValuePair<string,CharacterStatsData> _meepleCharacter)
        {
            //instantiate gameobject

            if (loadedPlayableMeeple != null)
            {
                var newPlayerMeeple = loadedPlayableMeeple.Clone();
                newPlayerMeeple.transform.position = new Vector3(0,newPlayerMeeple.transform.localScale.y / 2,0);
                var playerMeeple = newPlayerMeeple.GetComponent<PlayableCharacter>();
                playerMeeple.AssignStats(_meepleCharacter.Value);
                playerMeeple.InitializeCharacter();
                PlayerMeepleCreated?.Invoke(playerMeeple, _meepleCharacter.Value);
                return;
            }
            
            
            var handle = Addressables.LoadAssetAsync<GameObject>(meepleAsset);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newMeepleObject = Instantiate(handle.Result);
                    loadedEnemyMeeple = handle.Result;
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
            if (loadedEnemyMeeple != null)
            {
                var newEnemyMeeple = loadedEnemyMeeple.Clone();
                newEnemyMeeple.transform.position = new Vector3(0,newEnemyMeeple.transform.localScale.y / 2,0);
                var enemyMeeple = newEnemyMeeple.GetComponent<EnemyCharacterMeeple>();
                enemyMeeple.AssignStats(_meepleStats);
                enemyMeeple.InitializeCharacter();
                return;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(enemyMeeplAsset);
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    var newMeepleObject = Instantiate(handle.Result);
                    loadedEnemyMeeple = handle.Result;
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

        public Dictionary<string, CharacterStatsData> GetAllMeeples()
        {
            return m_allOwnedCharacters;
        }

        public void CacheMeepleGameObject(GameObject _meepleCharacter)
        {
            if (_meepleCharacter == null)
            {
                return;
            }
            
            
            m_cachedMeepleObjs.Add(_meepleCharacter);
            
            _meepleCharacter.transform.ResetTransform(cachedMeepleObjPool);
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