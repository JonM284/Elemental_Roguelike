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
        
        //[SerializeField] private Image highDetailStatus;
        
        [Header("Low Detail Bar")]

        [SerializeField] private GameObject lowDetail;

        [SerializeField] private Slider healthBarLow;
        
        //[SerializeField] private Image lowDetailStatus;

        [Header("Both")]

        [SerializeField] private List<GameObject> shieldIcons = new List<GameObject>();
        
        #endregion

        #region Private Fields

        private CharacterBase m_associatedCharacter;

        private Transform m_healthBarFollow;

        private RectTransform m_rect;

        private float yOffet = 0.35f;
        
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
            CharacterLifeManager.OnCharacterHealthChange += CharacterLifeManagerOnOnCharacterHealthChange;
            CharacterBase.StatusAdded += CharacterBaseOnStatusAdded;
        }

        private void OnDisable()
        {
            CharacterBase.CharacterHovered -= CharacterBaseOnCharacterHovered;
            CharacterLifeManager.OnCharacterHealthChange -= CharacterLifeManagerOnOnCharacterHealthChange;
            CharacterBase.StatusAdded -= CharacterBaseOnStatusAdded;
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
            
            CharacterLifeManagerOnOnCharacterHealthChange();
        }
        
        private void CharacterLifeManagerOnOnCharacterHealthChange()
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
        
        private void CharacterBaseOnCharacterHovered(bool _isHighlighted, CharacterBase _character)
        {
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
            
            //ToDo: Display status
        }

        private void ChangeHealthBarDisplay(bool _isHighlighted)
        {
            lowDetail.SetActive(!_isHighlighted);
            highDetail.SetActive(_isHighlighted);
        }

        #endregion



    }
}