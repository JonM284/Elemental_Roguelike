using System;
using System.Collections.Generic;
using Project.Scripts.Utils;
using Runtime.Character;
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
        
        [SerializeField] private TMP_Text healthText;
        
        [SerializeField] private TMP_Text shieldText;

        [SerializeField] private Image highStatusIcon;

        [Header("Low Detail Bar")]

        [SerializeField] private GameObject lowDetail;

        [SerializeField] private Slider healthBarLow;

        [SerializeField] private Image lowStatusIcon;
        
        [Header("Both")] 
        
        [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();
        
        #endregion

        #region Private Fields

        private CharacterBase m_associatedCharacter;

        private Transform m_healthBarFollow;

        private RectTransform m_rect;

        private float yOffet = 0.35f;

        //ToDo: Using this, make a nice animation of losing health
        private float m_previousHealth;
        private bool m_hasTakenDamage;
        
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
            CharacterBase.CharacterReset += OnCharacterReset;
            CharacterBase.StatusAdded += CharacterBaseOnStatusAdded;
            CharacterBase.StatusRemoved += CharacterBaseOnStatusRemoved;
        }

        private void OnDisable()
        {
            CharacterBase.CharacterHovered -= CharacterBaseOnCharacterHovered;
            CharacterLifeManager.OnCharacterHealthChange -= OnCharacterHealthChange;
            CharacterLifeManager.OnCharacterDied -= OnCharacterDied;
            CharacterBase.CharacterReset -= OnCharacterReset;
            CharacterBase.StatusAdded -= CharacterBaseOnStatusAdded;
            CharacterBase.StatusRemoved -= CharacterBaseOnStatusRemoved;
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
            
            OnCharacterHealthChange();
        }
        
        private void OnCharacterHealthChange()
        {
            if (m_associatedCharacter.IsNull())
            {
                return;
            }

            healthText.text = $"{currentHealth}/{maxHealth}";

            healthBarLow.value = healthPercentage;
            healthBarHigh.value = healthPercentage;

            shieldText.text = currentShield.ToString();

            shieldIcons.ForEach(g => g.SetActive(hasShield));

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
        
        private void CharacterBaseOnStatusAdded(CharacterBase _character)
        {
            if (_character != m_associatedCharacter)
            {
                return;
            }

            if (_character.appliedStatus.IsNull())
            {
                return;
            }

            if (_character.appliedStatus.status.statusIcon.IsNull())
            {
                return;
            }

            if (_character.appliedStatus.status.statusIconLow.IsNull())
            {
                return;
            }

            highStatusIcon.sprite = _character.appliedStatus.status.statusIcon;

            lowStatusIcon.sprite = _character.appliedStatus.status.statusIconLow;

        }
        
        private void CharacterBaseOnStatusRemoved(CharacterBase _character)
        {
            if (_character.IsNull())
            {
                return;
            }
            
            if (_character != m_associatedCharacter)
            {
                return;
            }

            highStatusIcon.sprite = null;

            lowStatusIcon.sprite = null;
        }

        private void ChangeHealthBarDisplay(bool _isHighlighted)
        {
            lowDetail.SetActive(!_isHighlighted);
            highDetail.SetActive(_isHighlighted);
        }

        #endregion



    }
}