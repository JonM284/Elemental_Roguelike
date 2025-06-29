﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.UI.Items;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI.DataModels
{
    public class BattleUIDataModel: MonoBehaviour
    {

        #region Nested Classes
        
        [Serializable]
        public class AbilityCooldowns
        {
            public GameObject cooldownImage;
            public Image radialCountdownImage;
            public TMP_Text countdownText;
        }

        #endregion

        #region Serialized Fields

        [SerializeField] private UIBase window;

        [SerializeField] private GameObject playerVisuals;

        [SerializeField] private GameObject selectedCharacterVisuals;

        [SerializeField] private GameObject shootButton;

        [SerializeField] private GameObject tooltipMenu;
        
        [SerializeField] private TMP_Text tooltipText;

        [SerializeField] private UIPopupCreator uiPopupCreator;

        [SerializeField] private AssetReference healthbarUI;
        
        [SerializeField] private AssetReference displaybarUI;

        [SerializeField] private Transform healthBarParent;

        [SerializeField] private Transform displayBarParent;
        
        [SerializeField] private string m_playerSideRef;

        [SerializeField] private List<GameObject> abilityButtons = new List<GameObject>();

        [SerializeField] private List<AbilityCooldowns> m_abilityCooldownImages = new List<AbilityCooldowns>();

        [SerializeField] private AbilityCooldowns m_overwatchCooldown;
        
        [Header("Timer")]
        
        [SerializeField] private Image timerSlider;

        [SerializeField] private TMP_Text timerText;

        [Header("ScoreBoard")] 
        
        [SerializeField] private TMP_Text m_blueScoreText;
 
        [SerializeField] private TMP_Text m_redScoreText;

        [SerializeField] private TMP_Text m_turnCounterText;
        
        #endregion

        #region Private Fields

        private bool isPlayerTurn;

        private bool m_isLoadingHealthBar;

        private bool m_isLoadingDisplayBar;

        private GameObject loadedHealthBarGO;

        private GameObject loadedDisplayBarGO;

        private List<GameObject> m_activeHealthbars = new List<GameObject>();

        private List<GameObject> m_cachedHealthbars = new List<GameObject>();

        private List<GameObject> m_activeTeamHealthBars = new List<GameObject>();

        private List<GameObject> m_cachedTeamHealthBars = new List<GameObject>();

        private CharacterSide m_playerSide;

        private float currentTime = 0;

        private float maxTimer = 60f;

        private bool m_isOutOfTime = false;

        #endregion

        #region Accessors

        public CharacterBase activePlayer => TurnUtils.GetActiveCharacter();

        public bool canDoAction => isPlayerTurn && 
                                   !activePlayer.IsNull() &&!activePlayer.isBusy;

        public CharacterSide playerSide => CommonUtils.GetRequiredComponent(ref m_playerSide, () =>
        {
            var _side = ScriptableDataController.Instance.GetSideByGuid(m_playerSideRef);
            return _side;
        });

        #endregion

        #region Unity Events

        private void Start()
        {
            if (loadedHealthBarGO.IsNull())
            {
                StartCoroutine(C_LoadHealthBar());
            }

            if (loadedDisplayBarGO.IsNull())
            {
                StartCoroutine(C_LoadDisplayBar());
            }
        }

        private void Update()
        {
            if (!isPlayerTurn)
            {
                return;
            }

            if (m_isOutOfTime)
            {
                return;
            }

            if (currentTime > 0 && !m_isOutOfTime)
            {
                currentTime -= Time.deltaTime;
                timerSlider.fillAmount = currentTime / maxTimer;
                int seconds = ((int)currentTime % 60);
                int minutes = ((int)currentTime / 60);
                timerText.text = string.Format("{0:00}:{1:00}",minutes, seconds);
            }
            else if(currentTime <= 0 && !m_isOutOfTime)
            {
                StopTimer();
                OnTimerFinished();
            }
            
            
        }

        private void OnEnable()
        {
            CharacterGameController.CharacterCreated += OnCharacterCreated;
            TurnController.OnBattleEnded += OnBattleEnded;
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
            CharacterBase.BallPickedUp += OnBallPickedUp;
            CharacterAbilityManager.ActionUsed += OnAbilityUsed;
            WinConditionController.PointScored += OnPointScored;
            WinConditionController.TurnCounterChanged += OnTurnCounterChanged;
        }

        private void OnDisable()
        {
            CharacterGameController.CharacterCreated -= OnCharacterCreated;
            TurnController.OnBattleEnded -= OnBattleEnded;   
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            CharacterBase.BallPickedUp -= OnBallPickedUp;
            CharacterAbilityManager.ActionUsed -= OnAbilityUsed;
            WinConditionController.PointScored -= OnPointScored;
            WinConditionController.TurnCounterChanged -= OnTurnCounterChanged;
        }

        #endregion

        #region Class Implementation

        private void OnCharacterCreated(CharacterBase _character)
        {
            if (_character.side == playerSide)
            {
                AddDisplayBar(_character);
            }
            
            AddHealthBar(_character);
        }

        private void AddHealthBar(CharacterBase _character)
        {
            StartCoroutine(C_AddHealthBar(_character));
        }

        private void AddDisplayBar(CharacterBase _character)
        {
            StartCoroutine(C_AddDisplayBar(_character));
        }

        private IEnumerator C_AddHealthBar(CharacterBase _character)
        {
            if (loadedHealthBarGO.IsNull())
            {
                Debug.Log("<color=blue>Waiting</color>");
                yield return new WaitUntil(() => m_isLoadingHealthBar == false);
                Debug.Log("<color=blue>WAITING FINISHED</color>");
            }
            
            var newHealthBar = m_cachedHealthbars.Count > 0 ? m_cachedHealthbars.FirstOrDefault() : loadedHealthBarGO.Clone(healthBarParent);
           
            if (newHealthBar.IsNull())
            {
                Debug.Log("Health Bar null");
            }
            
            newHealthBar.SetActive(true);
            
            if (m_cachedHealthbars.Contains(newHealthBar))
            {
                m_cachedHealthbars.Remove(newHealthBar);
            }
            
            newHealthBar.TryGetComponent(out HealthBarItem item);
            if (item)
            {
                item.Initialize(_character);
            }
            
            m_activeHealthbars.Add(newHealthBar);

        }

        private IEnumerator C_LoadHealthBar()
        {
            m_isLoadingHealthBar = true;

            var handle = Addressables.LoadAssetAsync<GameObject>(healthbarUI);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedHealthBarGO = handle.Result;
                for (int i = 0; i < 10; i++)
                {
                    var hb= loadedHealthBarGO.Clone(healthBarParent);
                    hb.SetActive(false);
                    m_cachedHealthbars.Add(hb);
                }
            }else{
                Debug.LogError($"Could not load addressable, {healthbarUI.Asset.name}", healthbarUI.Asset);
                Addressables.Release(handle);
            }

            m_isLoadingHealthBar = false;
        }

        private IEnumerator C_AddDisplayBar(CharacterBase _character)
        {
            if (loadedDisplayBarGO.IsNull())
            {
                yield return new WaitUntil(() => m_isLoadingDisplayBar == false);
            }
            
            var newDisplayBar = m_cachedTeamHealthBars.Count > 0 ? m_cachedTeamHealthBars.FirstOrDefault() : loadedDisplayBarGO.Clone(displayBarParent);

            if (newDisplayBar.IsNull())
            {
                Debug.Log("Display Bar null");
            }
            
            newDisplayBar.SetActive(true);
            
            if (m_cachedTeamHealthBars.Contains(newDisplayBar))
            {
                m_cachedTeamHealthBars.Remove(newDisplayBar);
            }
            
            newDisplayBar.TryGetComponent(out TeamHealthBarItem item);
            if (item)
            {
                item.Initialize(_character);
            }
            
            m_activeTeamHealthBars.Add(newDisplayBar);
        }

        private IEnumerator C_LoadDisplayBar()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(displaybarUI);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedDisplayBarGO = handle.Result;
                
                for (int i = 0; i < 5; i++)
                {
                    var hb= loadedDisplayBarGO.Clone(displayBarParent);
                    hb.SetActive(false);
                    m_cachedTeamHealthBars.Add(hb);
                }
            }else{
                Debug.LogError($"Could not load addressable, {healthbarUI.Asset.name}", healthbarUI.Asset);
                Addressables.Release(handle);
            }
        }
        
        private void OnPointScored(int _blueScore, int _redScore)
        {
            m_blueScoreText.text = _blueScore.ToString();
            m_redScoreText.text = _redScore.ToString();

            StopTimer();
        }
        
        private void OnTurnCounterChanged(int _newTurnAmount)
        {
            m_turnCounterText.text = _newTurnAmount.ToString();
        }

        private void OnBattleEnded()
        {
            m_activeHealthbars.ForEach(g =>
            {
                m_cachedHealthbars.Add(g);
                g.SetActive(false);
            });
            
            m_activeHealthbars.Clear();
            
            m_activeTeamHealthBars.ForEach(g =>
            {
                m_cachedTeamHealthBars.Add(g);
                g.SetActive(false);
            });
            
            m_activeTeamHealthBars.Clear();
            
            window.Close();
        }
        
        private void OnChangeCharacterTurn(CharacterBase _character)
        {
            var _isPlayerTurn = _character.side == playerSide;
            
            selectedCharacterVisuals.SetActive(_isPlayerTurn);
            
            if (_isPlayerTurn)
            {
                shootButton.SetActive(!_character.heldBall.IsNull());
            }

            if (!_isPlayerTurn)
            {
                return;
            }

            CheckAbilities();
        }
        
        private void OnBallPickedUp(CharacterBase _character)
        {
            if (_character == activePlayer)
            {
                shootButton.SetActive(!_character.heldBall.IsNull());
            }
        }

        public void OnMoveClicked()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.SetCharacterWalkAction();
        }

        public void OnAttackClicked()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterWeapon();
        }

        public void UseFirstAbility()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterAbility(0);
        }

        public void DescribeAbility(int _index)
        {
            if (!canDoAction)
            {
                return;
            }

            var highlightedAbility = activePlayer.characterAbilityManager.GetAbilityAtIndex(_index);
            tooltipMenu.SetActive(true);
            tooltipText.text = $"{highlightedAbility.abilityName}: <br> {highlightedAbility.abilityDescription} <br> " +
                               $"Target Type: {highlightedAbility.targetType} <br> Cooldown: {highlightedAbility.roundCooldownTimer} Turn(s)";
        }

        public void SetOverwatch()
        {
            if (!canDoAction)
            {
                return;
            }

            activePlayer.SetOverwatch();
        }

        public void DescribeOverwatch()
        {
            if (!canDoAction)
            {
                return;
            }
            
            tooltipMenu.SetActive(true);
            tooltipText.text = $"Class: {activePlayer.characterClassManager.assignedClass.name} <br> {activePlayer.characterClassManager.assignedClass.GetOverwatchDescription()}";
        }

        public void CloseToolTipMenu()
        {
            tooltipMenu.SetActive(false);
        }

        public void UseSecondAbility()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.UseCharacterAbility(1);
        }

        public void UseShootBall()
        {
            if (!canDoAction)
            {
                return;
            }
            
            activePlayer.SetCharacterThrowAction();
        }

        public void EndTurn()
        {
            var activeTeamMembers = TurnController.Instance.GetActiveTeam();
            if (!activeTeamMembers.TrueForAll(cb => cb.characterActionPoints == 0))
            {
                uiPopupCreator.CreatePopup();
            }
            else
            {
                ConfirmEndTurn();
            }
        }

        public void ConfirmEndTurn()
        {
            var activeTeamMembers = TurnController.Instance.GetActiveTeam();
            activeTeamMembers.ForEach(cb => cb.EndTurn());
            Debug.Log("<color=red>Player Ended Turn</color>");
            TurnController.Instance.EndTeamTurn();
        }
        
        private void OnChangeActiveTeam(CharacterSide characterSide)
        {
            isPlayerTurn = characterSide == playerSide;   
            
            playerVisuals.SetActive(isPlayerTurn);
            
            if (isPlayerTurn)
            {
                ResetTimer();
            }
        }
        
        private void OnAbilityUsed(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                Debug.Log("Character Null");
                return;
            }
            
            if (_character.side != playerSide)
            {
                Debug.Log("Character not Player");
                return;
            }   
            
            CheckAbilities();
        }

        private void CheckAbilities()
        {
            var activePlayerAbilities = activePlayer.characterAbilityManager.GetAssignedAbilities();

            abilityButtons.ForEach(g => g.SetActive(false));
            
            for (int i = 0; i < activePlayerAbilities.Count; i++)
            {
                abilityButtons[i].SetActive(true);
                m_abilityCooldownImages[i].cooldownImage.SetActive(!activePlayerAbilities[i].canUse);

                if (!activePlayerAbilities[i].canUse)
                {
                    m_abilityCooldownImages[i].countdownText.text = $"{activePlayerAbilities[i].roundCooldown}";
                    m_abilityCooldownImages[i].radialCountdownImage.fillAmount =
                        activePlayerAbilities[i].roundCooldownPercentage;
                }
            }

            m_overwatchCooldown.cooldownImage.SetActive(activePlayer.characterClassManager.overwatchCoolDown > 0);

            if (activePlayer.characterClassManager.overwatchCoolDown <= 0)
            {
                return;
            }
            
            m_overwatchCooldown.countdownText.text = $"{activePlayer.characterClassManager.overwatchCoolDown}";
            m_overwatchCooldown.radialCountdownImage.fillAmount =
                activePlayer.characterClassManager.overwatchCooldownPrct;

        }

        private void ResetTimer()
        {
            currentTime = maxTimer;
            m_isOutOfTime = false;
        }

        private void StopTimer()
        {
            m_isOutOfTime = true;
        }

        private void OnTimerFinished()
        {
            UIController.Instance.CloseAllPopups();
            ConfirmEndTurn();
        }

        #endregion

    }    
}

