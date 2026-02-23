using System;
using System.Collections.Generic;
using Data.Sides;
using Data.StatusDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.GameControllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI.Items
{
    public class HealthBarItem: MonoBehaviour
    {

        #region Serialized Fields
        
        [Header("High Detail Bar")]
        
        [SerializeField] private GameObject highDetail;
        
        [SerializeField] private Slider healthBarHigh;

        [SerializeField] private Image healthBarHighImage;
        
        [SerializeField] private TMP_Text healthText;
        
        [SerializeField] private TMP_Text shieldText;

        [SerializeField] private List<GameObject> highStatusIconParents = new List<GameObject>();
        [SerializeField] private List<Image> highStatusIcon = new List<Image>();

        [Header("Low Detail Bar")]

        [SerializeField] private GameObject lowDetail;

        [SerializeField] private Slider healthBarLow;

        [SerializeField] private List<GameObject> lowStatusIconParents = new List<GameObject>();
        [SerializeField] private List<Image> lowStatusIcon = new List<Image>();
        
        [SerializeField] private Image healthBarLowImage;

        [Header("Both")] 
        
        [SerializeField] private Color playerBarColor;
        
        [SerializeField] private Color enemyBarColor;
        
        [SerializeField] private List<GameObject> captainStars = new List<GameObject>();

        [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();

        [SerializeField] private List<GameObject> actionPointSmall = new List<GameObject>();

        [SerializeField] private List<GameObject> actionPointLarge = new List<GameObject>();

        #endregion

        #region Private Fields

        private CharacterBase m_associatedCharacter;

        private Transform m_healthBarFollow;

        private RectTransform m_rect;

        private float yOffet = 0.35f;

        //ToDo: Using this, make a nice animation of losing health
        private float m_previousHealth;
        private bool m_hasTakenDamage;
        
        private List<StatusData> appliedStatuses = new List<StatusData>();
        
        #endregion

        #region Accessors

        public int maxHealth => m_associatedCharacter.characterLifeManager.maxHealthPoints;

        public int currentHealth => m_associatedCharacter.characterLifeManager.currentHealthPoints;

        public int currentShield => m_associatedCharacter.characterLifeManager.currentShieldPoints;

        public float healthPercentage => (float)currentHealth / maxHealth;

        public bool hasShield => currentShield > 0;

        public UnityEngine.Camera cameraRef => CameraUtils.GetMainCamera();

        private RectTransform rectTransform => CommonUtils.GetRequiredComponent(ref m_rect, () =>
        {
            var rt = GetComponent<RectTransform>();
            return rt;
        });

        private Transform characterFollowTransform => CommonUtils.GetRequiredComponent(ref m_healthBarFollow, () =>
        {
            var t = m_associatedCharacter.characterLifeManager.healthBarFollowPos;
            return t;
        });

        #endregion

        #region Unity Events

        private void LateUpdate()
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (characterFollowTransform.IsNull())
            {
                return;
            }

            Vector3 screenPos =
                cameraRef.WorldToScreenPoint(new Vector3(characterFollowTransform.position.x, characterFollowTransform.position.y + yOffet, characterFollowTransform.position.z));

            rectTransform.position = screenPos;
        }

        private void OnEnable()
        {
            CharacterBase.CharacterHovered += CharacterBaseOnCharacterHovered;
            CharacterLifeManager.OnCharacterHealthChange += OnCharacterHealthChange;
            CharacterLifeManager.OnCharacterDied += OnCharacterDied;
            CharacterBase.CharacterUsedActionPoint += OnCharacterUsedActionPoint;
            CharacterBase.CharacterReset += OnCharacterReset;
            CharacterBase.StatusAdded += OnStatusApplied;
            CharacterBase.StatusRemoved += OnStatusRemoved;
            CharacterBase.CharacterEndedTurn += OnCharacterEndedTurn;
            TurnController.OnChangeActiveTeam += OnChangeActiveTeam;
        }

        private void OnDisable()
        {
            CharacterBase.CharacterHovered -= CharacterBaseOnCharacterHovered;
            CharacterLifeManager.OnCharacterHealthChange -= OnCharacterHealthChange;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
            CharacterBase.CharacterUsedActionPoint -= OnCharacterUsedActionPoint;
            CharacterBase.CharacterReset -= OnCharacterReset;
            CharacterBase.StatusAdded -= OnStatusApplied;
            CharacterBase.StatusRemoved -= OnStatusRemoved;
            CharacterBase.CharacterEndedTurn -= OnCharacterEndedTurn;
            TurnController.OnChangeActiveTeam -= OnChangeActiveTeam;

        }

        #endregion
        
        #region Class Implementation

        public void Initialize(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }

            m_associatedCharacter = _character;
            
            Debug.Log("Connected");
            
            OnCharacterHealthChange(m_associatedCharacter);
        }
        
        private void OnChangeActiveTeam(CharacterSide _side)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_side != m_associatedCharacter.side)
            {
                actionPointSmall.ForEach(g => g.SetActive(false));
                actionPointLarge.ForEach(g => g.SetActive(false));
                return;
            }

            for (int i = 0; i < m_associatedCharacter.characterActionPoints; i++)
            {
                actionPointSmall[i].SetActive(true);
                actionPointLarge[i].SetActive(true);
            }
        }
        
        private void OnCharacterUsedActionPoint(CharacterBase _character, int _amountLeft)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }

            for (int i = 0; i < actionPointSmall.Count; i++)
            {
                if (i == _amountLeft)
                {
                    actionPointSmall[i].SetActive(false);
                }
            }
            
            for (int i = 0; i < actionPointLarge.Count; i++)
            {
                if (i == _amountLeft)
                {
                    actionPointLarge[i].SetActive(false);
                }
            }
        }
        
        private void OnCharacterEndedTurn(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }

            actionPointSmall.ForEach(g => g.SetActive(false));
            actionPointLarge.ForEach(g => g.SetActive(false));
        }
        
        private void OnCharacterHealthChange(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }
            
            if (_character != m_associatedCharacter)
            {
                return;
            }

            /*healthText.text = $"{currentHealth}/{maxHealth}";

            healthBarLow.value = healthPercentage;
            healthBarHigh.value = healthPercentage;

            var _isPlayer = _character.side.sideGUID == TurnController.Instance.playersSide.sideGUID;

            healthBarHighImage.color = _isPlayer ? playerBarColor : enemyBarColor;
            
            healthBarLowImage.color = _isPlayer ? playerBarColor : enemyBarColor;

            shieldText.text = currentShield.ToString();

            shieldIcons.ForEach(g => g.SetActive(hasShield));
            
            captainStars.ForEach(g => g.SetActive(_character.characterStatsBase.isCaptain));*/
        }
        
        private void OnCharacterDied(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }
            
            lowDetail.SetActive(false);
            highDetail.SetActive(false);
        }
        
        private void OnCharacterReset(CharacterBase _character)
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            if (_character != m_associatedCharacter)
            {
                return;
            }
            
            lowDetail.SetActive(true);
            highDetail.SetActive(false);
        }
        
        private void CharacterBaseOnCharacterHovered(bool _isHighlighted, CharacterBase _character)
        {
            if (!m_associatedCharacter.isAlive)
            {
                return;
            }
            
            if (_character != this.m_associatedCharacter)
            {
                ChangeHealthBarDisplay(false);
                return;
            }
            
            ChangeHealthBarDisplay(_isHighlighted);
            
        }
        
        private void OnStatusApplied(CharacterBase characterBase, StatusData _status)
        {
            if (characterBase != m_associatedCharacter)
            {
                return;
            }
            
            appliedStatuses.Add(_status);
            UpdateStatusList();
        }

        private void OnStatusRemoved(CharacterBase characterBase, StatusData _status)
        {
            if (characterBase != m_associatedCharacter)
            {
                return;
            }

            if (!appliedStatuses.Contains(_status))
            {
                return;
            }
            
            appliedStatuses.Remove(_status);
            UpdateStatusList();
        }

        private void UpdateStatusList()
        {
            //ToDo: check if necessary. Health bar might be removed
            for (int i = 0; i < highStatusIconParents.Count; i++)
            {
                highStatusIconParents[i].gameObject.SetActive(i < appliedStatuses.Count);
                lowStatusIconParents[i].gameObject.SetActive(i < appliedStatuses.Count);

                if (i >= appliedStatuses.Count)
                {
                    continue;
                }

                highStatusIcon[i].sprite = appliedStatuses[i].statusIcon;
                lowStatusIcon[i].sprite = appliedStatuses[i].statusIconLow;
            }
        }

        private void ClearAllStatuses()
        {
            appliedStatuses.Clear();
        }
        
        
        private void ChangeHealthBarDisplay(bool _isHighlighted)
        {
            lowDetail.SetActive(!_isHighlighted);
            highDetail.SetActive(_isHighlighted);
        }

        #endregion



    }
}