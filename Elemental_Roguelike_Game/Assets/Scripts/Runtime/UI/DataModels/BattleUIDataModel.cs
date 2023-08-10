using System.Collections;
using Data.CharacterData;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.UI.Items;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utils;

namespace Runtime.UI.DataModels
{
    public class BattleUIDataModel: MonoBehaviour
    {

        #region Serialized Fields

        [SerializeField] private UIBase window;

        [SerializeField] private GameObject playerVisuals;

        [SerializeField] private GameObject shootButton;

        [SerializeField] private UIPopupCreator uiPopupCreator;

        [SerializeField] private AssetReference healthbarUI;

        [SerializeField] private Transform healthBarParent;
        
        [SerializeField] private CharacterSide playerSide;

        #endregion

        #region Private Fields

        private bool isPlayerTurn;

        private bool m_isLoadingHealthBar;

        private GameObject loadedHealthBarGO;
        
        #endregion

        #region Accessors

        public CharacterBase activePlayer => TurnUtils.GetActiveCharacter();

        public bool canDoAction => isPlayerTurn && 
                                   !activePlayer.IsNull() &&!activePlayer.isBusy;

        #endregion

        #region Unity Events

        private void Start()
        {
            StartCoroutine(C_LoadHealthBar());
        }

        private void OnEnable()
        {
            MeepleController.PlayerMeepleCreated += MeepleControllerOnPlayerMeepleCreated;
            EnemyController.EnemyCreated += EnemyControllerOnEnemyCreated;
            TurnController.OnBattleEnded += OnBattleEnded;
            TurnController.OnChangeActiveCharacter += OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
            CharacterBase.BallPickedUp += OnBallPickedUp;
        }

        private void OnDisable()
        {
            MeepleController.PlayerMeepleCreated -= MeepleControllerOnPlayerMeepleCreated;
            EnemyController.EnemyCreated -= EnemyControllerOnEnemyCreated;
            TurnController.OnBattleEnded -= OnBattleEnded;   
            TurnController.OnChangeActiveCharacter -= OnChangeCharacterTurn;
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;
            CharacterBase.BallPickedUp -= OnBallPickedUp;
        }

        #endregion

        #region Class Implementation
        
        private void EnemyControllerOnEnemyCreated(CharacterBase _enemy)
        {
            if (_enemy.IsNull())
            {
                return;
            }
            
            AddHealthBar(_enemy);
        }

        private void MeepleControllerOnPlayerMeepleCreated(CharacterBase _meeple, CharacterStatsData _stats)
        {
            if (_meeple.IsNull())
            {
                return;
            }
            
            AddHealthBar(_meeple);
        }

        private void AddHealthBar(CharacterBase _character)
        {
            StartCoroutine(C_AddHealthBar(_character));
        }

        private IEnumerator C_AddHealthBar(CharacterBase _character)
        {
            if (loadedHealthBarGO.IsNull())
            {
                if (!m_isLoadingHealthBar)
                {
                    yield return StartCoroutine(C_LoadHealthBar());
                }
                else
                {
                    yield return new WaitUntil(() => m_isLoadingHealthBar = false);
                }
            }
            
            var newHealthBar = loadedHealthBarGO.Clone(healthBarParent);
            newHealthBar.TryGetComponent(out HealthBarItem item);
            if (item)
            {
                item.Initialize(_character);
            }

        }

        private IEnumerator C_LoadHealthBar()
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(healthbarUI);
            
            Debug.Log("<color=#00FF00>Loading GameObject</color>");

            if (!handle.IsDone)
            {
                yield return handle;
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedHealthBarGO = handle.Result;
            }else{
                Debug.LogError($"Could not load addressable, {healthbarUI.Asset.name}", healthbarUI.Asset);
                Addressables.Release(handle);
            }
        }

        private void OnBattleEnded()
        {
            window.Close();
        }
        
        private void OnChangeCharacterTurn(CharacterBase _character)
        {
            isPlayerTurn = _character.side == playerSide;
            playerVisuals.SetActive(isPlayerTurn);
            if (isPlayerTurn)
            {
                shootButton.SetActive(!_character.heldBall.IsNull());
            }
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
            var _isPlayerTeam = characterSide == playerSide;   
            playerVisuals.SetActive(_isPlayerTeam);
        }

        #endregion

    }    
}

