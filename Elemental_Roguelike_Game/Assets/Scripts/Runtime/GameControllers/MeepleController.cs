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
    public class MeepleController: GameControllerBase
    {

        #region READ-ME

        //The Purpose of this controller is to create meeples, this should not save data related to teams
        //Create Meeple
        //If meeple is selected to be on a team, do that somewhere else

        #endregion
        
        #region Events

        public static event Action<CharacterBase, CharacterStatsData> PlayerMeepleCreated;
        
        #endregion

        #region Serialized Fields

        [SerializeField] private AssetReference meepleAsset;

        [SerializeField] private AssetReference enemyMeeplAsset;
        
        #endregion
        
        #region Private Fields
        
        private List<CharacterStatsData> m_randomGeneratedCharacters = new List<CharacterStatsData>();
        
        private List<GameObject> m_cachedMeepleObjs = new List<GameObject>();

        private GameObject loadedPlayableMeeple;

        private GameObject loadedEnemyMeeple;

        private int m_rerollsAmount = 2;

        private Transform m_cachedMeeplePoolTransform;

        private const int playerTeamSize = 5;

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
            LevelGenerationManager.LevelGenerationFinished += PlaceStartCharacters;
        }

        private void OnDisable()
        {
            LevelGenerationManager.LevelGenerationFinished -= PlaceStartCharacters;
        }

        #endregion
        
        #region Class Implementation

        public void ResetVariablesForNewRun()
        {
            m_rerollsAmount = 2;
            m_randomGeneratedCharacters.Clear();
        }

        private void CreateTeamForNewRun()
        {
            for (int i = 0; i < playerTeamSize; i++)
            {
                CreateNewCharacter();
            }
            
        }

        private void PlaceStartCharacters(RoomTracker _roomTracker)
        {
            if (meepleAsset == null)
            {
                Debug.LogError("Meeple asset not assigned");
                return;
            }

            var _savedTeam = TeamUtils.GetCurrentTeam();

            foreach (var teamMember in _savedTeam.teamMembers)
            {
                InstantiatePremadeMeeple(teamMember);
            }
            
        }

        public void DeletePlayerMeeple(string _meepleGUID)
        {
           
        }

        [ContextMenu("Clear all characters")]
        public void ClearRandomMadeCharacters()
        {
            m_randomGeneratedCharacters.Clear();
        }
        
        [ContextMenu("Create Character")]
        public void CreateNewCharacter()
        {
            if (m_randomGeneratedCharacters.Count >= 5)
            {
                return;
            }
            
            var newCharacter = new CharacterStatsData();
            
            RandomizeCharacterVariables(newCharacter);
            
            newCharacter.id = System.Guid.NewGuid().ToString();
            
            m_randomGeneratedCharacters.Add(newCharacter);
        }

        private void RandomizeCharacterVariables(CharacterStatsData _character)
        {
            _character.meepleElementTypeRef = ElementUtils.GetRandomElement().elementGUID;
            _character.initiativeNumber = Random.Range(1, 20);
            _character.baseDamage = Random.Range(1f, 10f);
            _character.baseHealth = Random.Range(10, 20);
            _character.baseShields = Random.Range(10, 20);
            _character.baseSpeed = 5f;
            _character.movementDistance = 3;
            for (int i = 0; i < 2; i++)
            {
                var randomAbility = AbilityUtils.GetRandomAbilityByType(_character.meepleElementTypeRef);
                _character.abilityReferences.Add(randomAbility.abilityGUID);
            }

            //All meeples start with pistol
            _character.weaponReference = WeaponUtils.GetDefaultWeapon().weaponGUID;
            //random element assigned to weapon
            _character.weaponElementTypeRef = ElementUtils.GetRandomElement().elementGUID;
        }

        public void InstantiatePremadeMeeple(CharacterStatsData _meepleCharacter)
        {
            
            //if asset is loaded, use loaded asset to instantiate
            if (loadedPlayableMeeple != null)
            {
                var newPlayerMeeple = loadedPlayableMeeple.Clone();
                newPlayerMeeple.transform.position = new Vector3(0,newPlayerMeeple.transform.localScale.y / 2,0);
                var playerMeeple = newPlayerMeeple.GetComponent<PlayableCharacter>();
                playerMeeple.AssignStats(_meepleCharacter);
                playerMeeple.InitializeCharacter();
                PlayerMeepleCreated?.Invoke(playerMeeple, _meepleCharacter);
                return;
            }
            
            //if asset is not loaded, 1. load asset, 2. Instantiate loaded asset
            
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
                            playableCharacter.AssignStats(_meepleCharacter);
                        }
                        newMeeple.InitializeCharacter();
                        PlayerMeepleCreated?.Invoke(newMeeple, _meepleCharacter);
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

    }
}