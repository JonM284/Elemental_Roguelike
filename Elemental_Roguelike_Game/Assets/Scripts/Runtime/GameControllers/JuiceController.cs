using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.UI.DataReceivers;
using Runtime.UI.Items;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utils;

namespace Runtime.GameControllers
{
    public class JuiceController: GameControllerBase
    {

        #region Static

        public static JuiceController Instance { get; private set; }

        #endregion

        #region Serialized Fields

        [SerializeField] private UIWindowData juiceUIWindowData;
        
        [SerializeField] private AssetReference damageTextRef;

        [SerializeField] private GameObject cameraHolder;
        
        [SerializeField] private float shakeStrength;

        [SerializeField] private int shakeAmplitude;

        [SerializeField] private Camera leftCharCam;

        [SerializeField] private Camera rightCharCam;

        [SerializeField] private Camera deathCam;

        [SerializeField] private Texture characterTex1;
        
        [SerializeField] private Texture characterTex2;
        
        [SerializeField] private Texture characterTex3;

        #endregion
        
        #region Private Fields

        private JuiceUIDataModel juiceUIDataModel;

        private DamageTextUIItem cachedDamageText;

        private List<DamageTextUIItem> cachedDamageTexts = new List<DamageTextUIItem>();

        private Transform m_textPool;

        private bool m_isShakingCamera;
        
        #endregion

        #region Acessors

        private Transform textPool => CommonUtils.GetRequiredComponent(ref m_textPool, () =>
        {
            var t = TransformUtils.CreatePool(this.transform, false);
            return t;
        });


        private Camera cameraRef => CameraUtils.GetMainCamera();

        public bool isDoingActionAnimation { get; private set; }

        #endregion

        #region Unity Events

        public void OnEnable()
        {
            TurnController.OnBattlePreStart += SetupJuiceUI;
        }

        private void OnDisable()
        {
            TurnController.OnBattlePreStart -= SetupJuiceUI;
        }

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

        private void SetupJuiceUI()
        {
            if (!is_Initialized)
            {
                return;
            }
            
            UIController.Instance.AddUICallback(juiceUIWindowData, InitializeJuiceUI);
            if (cachedDamageText.IsNull())
            {
                StartCoroutine(AddressableController.Instance.C_LoadGameObject(damageTextRef, SetupCachedDamageText, textPool));
            }
        }

        public Texture GetTexture1()
        {
            return characterTex1;
        }

        public Texture GetTexture2()
        {
            return characterTex2;
        }

        public Texture GetTexture3()
        {
            return characterTex3;
        }

        private void InitializeJuiceUI(GameObject _uiWindowGO)
        {
            _uiWindowGO.TryGetComponent(out JuiceUIDataModel uiDataModel);
            if (uiDataModel.IsNull())
            {
                return;
            }
            
            juiceUIDataModel = uiDataModel;
            Debug.Log("Has Created Juice UI", juiceUIDataModel);
        }

        private void SetupCachedDamageText(GameObject _returnedObj)
        {
            _returnedObj.TryGetComponent(out DamageTextUIItem damageTextUIItem);
            if (damageTextUIItem)
            {
                cachedDamageText = damageTextUIItem;
            }
        }

        public void DoCameraShake(float _duration, float _strength, int _amplitude, float randomness)
        {
            if (m_isShakingCamera)
            {
                return;
            }

            m_isShakingCamera = true;
            StartCoroutine(C_CameraShake(_duration, _strength, _amplitude, randomness));
        }

        private IEnumerator C_CameraShake(float _duration, float _strength, int _amplitude, float randomness)
        {
            cameraRef.DOShakePosition(_duration, _strength, _amplitude, randomness);

            yield return new WaitForSeconds(_duration);

            m_isShakingCamera = false;
            cameraRef.transform.localPosition = Vector3.zero;
        }

        public IEnumerator C_ScorePoint(bool _isPlayerGoal, Transform cameraPos)
        {
            cameraHolder.SetActive(true);

            isDoingActionAnimation = true;
            
            //scored on player, highlight enemy
            if (_isPlayerGoal)
            {
                rightCharCam.transform.position = cameraPos.position;
                rightCharCam.transform.forward = cameraPos.forward;
            }
            else
            {
                leftCharCam.transform.position = cameraPos.position;
                leftCharCam.transform.forward = cameraPos.forward;
            }
            
            DoCameraShake(3.45f, shakeStrength, shakeAmplitude, 90f);

            yield return StartCoroutine(juiceUIDataModel.C_ScoreGoal(_isPlayerGoal));

            isDoingActionAnimation = false;
            
            ResetCam();
        }


        public IEnumerator C_DoReactionAnimation(Transform LCameraPoint, Transform RCameraPoint, int _endValueL, int _endValueR, CharacterClass _characterClass, bool _isLeftReactor)
        {
            cameraHolder.SetActive(true);

            isDoingActionAnimation = true;
            
            leftCharCam.transform.position = LCameraPoint.position;
            leftCharCam.transform.forward = LCameraPoint.forward;
            
            rightCharCam.transform.position = RCameraPoint.position;
            rightCharCam.transform.forward = RCameraPoint.forward;
            
            yield return StartCoroutine(juiceUIDataModel.C_ReactionEvent(_endValueL, _endValueR, ResetCam, _characterClass, _isLeftReactor));

            isDoingActionAnimation = false;
            
            ResetCam();
        }

        public IEnumerator C_DoDeathAnimation(Transform _deadCharacterCameraPoint)
        {
            isDoingActionAnimation = true;
            
            CameraUtils.SetCameraTrackPos(_deadCharacterCameraPoint.position, true);
            
            cameraHolder.SetActive(true);
            deathCam.transform.position = _deadCharacterCameraPoint.position;
            deathCam.transform.forward = _deadCharacterCameraPoint.forward;
            
            yield return StartCoroutine(juiceUIDataModel.C_DeathUIEvent());

            isDoingActionAnimation = false;

            ResetCam();
        }

        public void ChangeSide(bool _isPlayerTurn)
        {
            StartCoroutine(juiceUIDataModel.C_ChangeSide(_isPlayerTurn));
        }

        private void ResetCam()
        {
            cameraHolder.SetActive(false);
        }

        public void CreateDamageText(int _amount, Vector3 _position)
        {
            if (cachedDamageTexts.Count > 0)
            {
                var foundText = cachedDamageTexts.FirstOrDefault();
                
                cachedDamageTexts.Remove(foundText);

                foundText.transform.parent = null;
                
                foundText.transform.position = _position;
                
                foundText.Initialize(_amount);
                return;
            }

            var createdText = Instantiate(cachedDamageText.gameObject, _position, Quaternion.identity);

            createdText.TryGetComponent(out DamageTextUIItem damageTextUIItem);

            if (damageTextUIItem)
            {
                damageTextUIItem.Initialize(_amount);
            }
        }

        public void CacheDamageText(DamageTextUIItem _item)
        {
            if (_item.IsNull())
            {
                return;
            }
            
            cachedDamageTexts.Add(_item);
            _item.transform.parent = textPool;
        }

        #endregion

    }
}