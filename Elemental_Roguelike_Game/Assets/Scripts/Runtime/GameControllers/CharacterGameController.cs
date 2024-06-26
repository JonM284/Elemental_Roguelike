﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Character;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

namespace Runtime.GameControllers
{
    public class CharacterGameController: GameControllerBase
    {
        #region Static

        public static CharacterGameController Instance { get; private set; }

        #endregion

        #region Events

        public static event Action<CharacterBase> CharacterCreated;

        #endregion

        #region Serialized Fields
        
        [SerializeField] private List<CharacterStatsBase> m_allCharacters = new List<CharacterStatsBase>();
        
        [SerializeField] private Color m_shotColor = Color.red;

        [SerializeField] private Color m_passColor = Color.green;

        [SerializeField] private int m_characterStatMax = 10;

        #endregion
        
        #region Private Fields

        private List<CharacterBase> m_cachedCharacters = new List<CharacterBase>();

        private List<CharacterBase> m_cachedLoadedCharacters = new List<CharacterBase>();

        private Transform m_cachedCharacterPoolTransform;

        public Color shotColor => m_shotColor;

        public Color passColor => m_passColor;
        
        #endregion

        #region Accessors

        public Transform cachedCharacterPool =>
            CommonUtils.GetRequiredComponent(ref m_cachedCharacterPoolTransform, ()=>
            {
                var poolTransform = TransformUtils.CreatePool(this.transform, false);
                poolTransform.RenameTransform("Character POOL");
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

        public int GetStatMax()
        {
            return m_characterStatMax;
        }

        public int GetMaxCharacterAmount()
        {
            return m_allCharacters.Count;
        }

        public List<CharacterStatsBase> GetAllCharacters()
        {
            return m_allCharacters;
        }
        
        public CharacterStatsBase GetCharacterByGUID(string _searchGUID)
        {
            return m_allCharacters.FirstOrDefault(csb => csb.characterGUID == _searchGUID);
        }

        private CharacterBase GetCachedCharacter(CharacterStatsBase _stats)
        {
            if (_stats == null)
            {
                return default;
            }

            return m_cachedCharacters.FirstOrDefault(cb => cb.characterStatsBase == _stats);
        }

        private CharacterBase GetLoadedCharacter(CharacterStatsBase _stats)
        {
            if (_stats == null)
            {
                return default;
            }

            return m_cachedLoadedCharacters.FirstOrDefault(cb => cb.characterStatsBase == _stats);
        }

        public IEnumerator C_CreateCharacter(CharacterStatsBase _characterStats, Vector3 _spawnPos, Vector3 spawnRotation, Action<CharacterBase> callback = null)
        {
            if (_characterStats == null)
            {
                yield break;
            }
            
            var adjustedSpawnLocation = _spawnPos != Vector3.zero ? _spawnPos : Vector3.zero;

            var adjustedSpawnRotation = spawnRotation != Vector3.zero ? spawnRotation : Vector3.zero;

            var foundCharacter = GetCachedCharacter(_characterStats);

            if (!foundCharacter.IsNull())
            {
                m_cachedCharacters.Remove(foundCharacter);
                foundCharacter.transform.parent = null;
                foundCharacter.transform.position = adjustedSpawnLocation;
                foundCharacter.transform.rotation = Quaternion.Euler(adjustedSpawnRotation);
                foundCharacter.InitializeCharacter(_characterStats);
                if (!callback.IsNull())
                {
                    callback?.Invoke(foundCharacter);
                }
                CharacterCreated?.Invoke(foundCharacter);
                yield break;
            }

            //Check if enemy was previously loaded
            var foundLoadedCharacter = GetLoadedCharacter(_characterStats);

            if (!foundLoadedCharacter.IsNull())
            {
                var _newCharacterGO = Instantiate(foundLoadedCharacter.gameObject, adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
                var _characterComp = _newCharacterGO.GetComponent<CharacterBase>();
                _characterComp.InitializeCharacter(_characterStats);
                CharacterCreated?.Invoke(_characterComp);
                yield break;
            }
            
            //Load new Enemy
            
            var handle = Addressables.LoadAssetAsync<GameObject>(_characterStats.characterAssetRef);
            yield return handle;
            
            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var _newCharacterObject = Instantiate(handle.Result, adjustedSpawnLocation, Quaternion.Euler(adjustedSpawnRotation));
                var _newCharacter = _newCharacterObject.GetComponent<CharacterBase>();
                m_cachedLoadedCharacters.Add(handle.Result.GetComponent<CharacterBase>());
                if (_newCharacter != null)
                {
                    _newCharacter.InitializeCharacter(_characterStats);
                }
                CharacterCreated?.Invoke(_newCharacter);
            }
            else
            {
                Addressables.Release(handle);
            }
            
        }

        public void CacheCharacter(CharacterBase _character)
        {
            if (_character == null)
            {
                return;
            }

            m_cachedCharacters.Add(_character);
            _character.transform.ResetTransform(cachedCharacterPool);
        }
        
        #endregion
    }
}