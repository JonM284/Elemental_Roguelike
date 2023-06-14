﻿using System;
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

        
        #region Class Implementation

        public void DeletePlayerMeeple(string _meepleGUID)
        {
           
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
            _character.initiativeNumber = Random.Range(1, 20);
            _character.baseDamage = Random.Range(1f, 10f);
            _character.baseHealth = Random.Range(10, 20);
            _character.baseShields = Random.Range(10, 20);
            _character.baseSpeed = 5f;
            //ToDo: Change movement distance
            _character.movementDistance = 3;
            
            for (int i = 0; i < 2; i++)
            {
                var randomAbility = AbilityUtils.GetRandomAbilityByType(_character.meepleElementTypeRef);
                _character.abilityReferences.Add(randomAbility.abilityGUID);
            }

            //ToDo: remove weapons
            //All meeples start with pistol
            _character.weaponReference = WeaponUtils.GetDefaultWeapon().weaponGUID;
            //random element assigned to weapon
            _character.weaponElementTypeRef = ElementUtils.GetRandomElement().elementGUID;
        }

        public void InstantiatePremadeMeeple(CharacterStatsData _meepleCharacter, Vector3 spawnLocation)
        {

            var adjustedSpawnLocation = spawnLocation != Vector3.zero ? spawnLocation : Vector3.zero;
            
            //if asset is loaded, use loaded asset to instantiate
            if (loadedPlayableMeeple != null)
            {
                var newPlayerMeeple = loadedPlayableMeeple.Clone();
                newPlayerMeeple.transform.position = adjustedSpawnLocation;
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
                    newMeepleObject.transform.position = adjustedSpawnLocation;
                    if (newMeepleObject.TryGetComponent(out CharacterBase newMeeple))
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

        //ToDo: not added yet, add after getting regular enemies to work
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