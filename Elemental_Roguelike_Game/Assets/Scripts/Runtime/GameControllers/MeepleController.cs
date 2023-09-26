using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
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
        
        #region Static

        public static MeepleController Instance { get; private set; }

        #endregion
        
        #region Events

        public static event Action<CharacterBase, CharacterStatsData> PlayerMeepleCreated;
        
        #endregion

        #region Serialized Fields

        [SerializeField] private AssetReference strikerMeepleAsset;
        
        [SerializeField] private AssetReference bruiserMeepleAsset;
        
        [SerializeField] private AssetReference defenderMeepleAsset;

        [SerializeField] private AssetReference enemyMeeplAsset;

        [SerializeField] private List<CharacterClassData> m_possibleClasses = new List<CharacterClassData>();

        #endregion
        
        #region Private Fields
        
        //Used to keep all meeple gameObjects in-game without deleting it.
        //Saves the need to re-instantiate another meeple, new meeple can use this gameobject but change details
        private List<GameObject> m_cachedMeepleObjs = new List<GameObject>();

        private GameObject loadedPlayableMeeple;

        private GameObject loadedEnemyMeeple;

        private int m_rerollsAmount = 2;

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

        public void DeletePlayerMeeple(string _meepleGUID)
        {
           
        }

        private CharacterClassData GetRandomClassType()
        {
            var randomInt = Random.Range(0, m_possibleClasses.Count);
            return m_possibleClasses[randomInt];
        }
        public CharacterClassData GetClassByGUID(string _guid)
        {
            return m_possibleClasses.FirstOrDefault(ccd => ccd.classGUID == _guid);
        }
        
        
        [ContextMenu("Create Character")]
        public CharacterStatsData CreateNewCharacter()
        {
            var newCharacter = new CharacterStatsData();
            
            RandomizeCharacterVariables(newCharacter);
            
            newCharacter.id = System.Guid.NewGuid().ToString();

            return newCharacter;
        }

        private void RandomizeCharacterVariables(CharacterStatsData _character)
        {
            _character.meepleElementTypeRef = ElementUtils.GetRandomElement().elementGUID;
            var randomClass = GetRandomClassType();
            _character.classReferenceType = randomClass.classGUID;
            _character.baseHealth = randomClass.GetRandomHealth();
            _character.currentHealth = _character.baseHealth;
            _character.baseShields = randomClass.GetRandomShield();
            _character.currentShield = _character.baseShields;
            _character.baseSpeed = 10f;
            _character.movementDistance = randomClass.GetMoveDistance();
            
            for (int i = 0; i < 2; i++)
            {
                var randomAbility = AbilityUtils.GetRandomAbilityByType(_character.meepleElementTypeRef, _character.classReferenceType);
                _character.abilityReferences.Add(randomAbility.abilityGUID);
            }

            var foundClass = GetClassByGUID(_character.classReferenceType);
            _character.agilityScore = foundClass.GetRandomAgilityScore();
            _character.shootingScore = foundClass.GetRandomShootingScore();
            _character.damageScore = foundClass.GetRandomDamageScore();
            _character.passingScore = foundClass.GetRandomPassingScore();
        }

        public IEnumerator InstantiatePremadeMeeple(CharacterStatsData _meepleCharacter, Vector3 spawnLocation, Vector3 spawnRotation)
        {
            yield return null;

            var correctClass = GetClassByGUID(_meepleCharacter.classReferenceType);
            AssetReference prefabByClass;

            switch (correctClass.classType)
            {
                case CharacterClass.STRIKER:
                    prefabByClass = strikerMeepleAsset;
                    break;
                case CharacterClass.BRUISER:
                    prefabByClass = bruiserMeepleAsset;
                    break;
                case CharacterClass.DEFENDER:
                    prefabByClass = defenderMeepleAsset;
                    break;
                default:
                    prefabByClass = strikerMeepleAsset;
                    break;
            }
            
            var adjustedSpawnLocation = spawnLocation != Vector3.zero ? spawnLocation : Vector3.zero;

            var adjustedSpawnRotation = spawnRotation != Vector3.zero ? spawnRotation : Vector3.zero;
            
            //if asset is loaded, use loaded asset to instantiate
            if (!loadedPlayableMeeple.IsNull())
            {
                Debug.Log("<color=orange>Already Loaded</color>");
                var newPlayerMeeple = prefabByClass.InstantiateAsync(adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
                yield return newPlayerMeeple;
                newPlayerMeeple.Result.transform.position = adjustedSpawnLocation;
                var playerMeeple = newPlayerMeeple.Result.GetComponent<PlayableCharacter>();
                playerMeeple.AssignStats(_meepleCharacter);
                playerMeeple.InitializeCharacter();
                PlayerMeepleCreated?.Invoke(playerMeeple, _meepleCharacter);
                yield break;
            }
            
            //if asset is not loaded, 1. load asset, 2. Instantiate loaded asset
            
            var handle = prefabByClass.InstantiateAsync(adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
            
            Debug.Log("<color=#00FF00>Loading Premade Meeple</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                //loadedPlayableMeeple = handle.Result;
                if (handle.Result.TryGetComponent(out CharacterBase newMeeple))
                {
                    if (newMeeple is PlayableCharacter playableCharacter)
                    {
                        playableCharacter.AssignStats(_meepleCharacter);
                    }
                    newMeeple.InitializeCharacter();
                    Debug.Log("MAKING LOADED MEEPLE - LOADING FINISHED");
                    PlayerMeepleCreated?.Invoke(newMeeple, _meepleCharacter);
                }
            }else {
                Addressables.Release(handle);
            }
        }

        //ToDo: not added yet, add after getting regular enemies to work
        public void InstantiateMeepleEnemy(CharacterStatsData _meepleStats)
        {
            if (!loadedEnemyMeeple.IsNull())
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