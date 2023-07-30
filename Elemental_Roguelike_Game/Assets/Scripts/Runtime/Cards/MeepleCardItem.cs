using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Selection;
using Runtime.UI.Items;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using Utils;

namespace Runtime.Cards
{
    public class MeepleCardItem : MonoBehaviour, ISelectable
    {
        #region Read-Only

        private static readonly int LightColor = Shader.PropertyToID("_WeaponColorR");
        
        private static readonly int DarkColor = Shader.PropertyToID("_WeaponColorB");

        #endregion
        
        #region Actions

        public static event Action<CharacterStatsData> MeepleItemSelected;

        #endregion

        #region SerializedFields

        [SerializeField] private CharacterClassData m_classData;
        
        [SerializeField] private TMP_Text shieldText;

        [SerializeField] private TMP_Text healthText;

        [SerializeField] private TMP_Text abilityDescriptionText;

        [SerializeField] private TMP_Text agilityScoreText;
        [SerializeField] private TMP_Text shootingScoreText;
        [SerializeField] private TMP_Text tacklingScoreText;

        [SerializeField] private Image agilitySliderImage;
        [SerializeField] private Image shootingSliderImage;
        [SerializeField] private Image tacklingSliderImage;

        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private SpriteRenderer elementIconRenderer;

        [SerializeField] private Transform meepleStandingLocation;

        [SerializeField] private Transform abilityDescriptionParent;

        [SerializeField] private GameObject abilityDescriptionPrefab;

        #endregion

        #region Private Fields

        private Material m_clonedMaterial;

        private Vector3 m_inHandPosition;

        private bool m_isMoving;

        private List<CardAbilityDescriptionItem> m_cachedDescriptionItems = new List<CardAbilityDescriptionItem>();
        
        private List<CardAbilityDescriptionItem> m_activeDescriptionItems = new List<CardAbilityDescriptionItem>();

        #endregion

        #region Accessors

        public bool isSelected { get; private set; }
        
        public CharacterStatsData assignedData { get; private set; }

        public Transform displayMeepleLocation => meepleStandingLocation;

        public GameObject assignedMeepleObj { get; private set; }

        public CharacterClassData classData => m_classData;

        #endregion
        
        #region Class Implementation
        
        public void InitializeSelectionItem(CharacterStatsData meeple)
        {
            if (meeple.IsNull())
            {
                Debug.LogError("ERROR with meeple data");
                return;
            }

            assignedData = meeple;

            InitializeVisualColors(meeple);

            //InitializeAbilityDescription(meeple);
            
            InitializeTexts(meeple);

            InitializeSliders(meeple);

            InitializeElementIcon(meeple);

            m_isMoving = true;
        }

        private void InitializeTexts(CharacterStatsData _data)
        {
            if (_data.IsNull())
            {
                Debug.LogError("DATA NULL");
                return;
            }
            
            shieldText.text = $"{_data.baseShields}";
            
            healthText.text = $"{_data.baseHealth}";
            
            agilityScoreText.text = $"{_data.agilityScore}";
            shootingScoreText.text = $"{_data.shootingScore}";
            tacklingScoreText.text = $"{_data.damageScore}";

            string combinedstring = "";

            _data.abilityReferences.ForEach(s =>
            {
                var ability = AbilityUtils.GetAbilityByGUID(s);
                var temp = $"{ability.abilityName}: {ability.abilityDescription}\r\n";
                combinedstring += temp;
            });

            abilityDescriptionText.text = combinedstring;

        }

        public void SetMovement(Vector3 _position, Vector3 _forward, bool _isFirstTime)
        {
            if (_isFirstTime)
            {
                m_inHandPosition = _position;
            }
            
            m_isMoving = true;
            MoveableController.Instance.CreateNewMoveable(this.transform, _position, _forward,
                OnFinishMovement);
        }

        private void InitializeSliders(CharacterStatsData _data)
        {
            agilitySliderImage.fillAmount = _data.agilityScore / 100f;
            shootingSliderImage.fillAmount = _data.shootingScore / 100f;
            tacklingSliderImage.fillAmount = _data.damageScore / 100f;
        }

        private void InitializeElementIcon(CharacterStatsData _data)
        {
            var _icon = ElementUtils.GetElementTypeByGUID(_data.meepleElementTypeRef).elementSprite;
            if (_icon.IsNull())
            {
                return;
            }

            elementIconRenderer.sprite = _icon;
        }

        private void InitializeVisualColors(CharacterStatsData _data)
        {
            if (meshRenderer.IsNull())
            {
                return;
            }

            
            var _type = ElementUtils.GetElementTypeByGUID(_data.meepleElementTypeRef);
            
            meshRenderer.materials[0].SetColor(LightColor, _type.meepleColors[0]);
            meshRenderer.materials[0].SetColor(DarkColor, _type.meepleColors[1]);
            
            //meshRenderer.materials[0] = m_clonedMaterial;
        }

        private void InitializeAbilityDescription(CharacterStatsData _data)
        {
            if (_data.abilityReferences.Count == 0)
            {
                return;
            }

            _data.abilityReferences.ForEach(s =>
            {
                var ability = AbilityUtils.GetAbilityByGUID(s);
                if (!ability.IsNull())
                {
                    GameObject descriptionGO = null;
                    if (m_cachedDescriptionItems.Count > 0)
                    {
                        descriptionGO = m_cachedDescriptionItems.FirstOrDefault().gameObject;
                        m_cachedDescriptionItems.RemoveAt(0);
                    }

                    if (descriptionGO.IsNull())
                    {
                        descriptionGO = abilityDescriptionPrefab.Clone(abilityDescriptionParent);
                    }
                    
                    descriptionGO.TryGetComponent(out CardAbilityDescriptionItem item);
                    if (item)
                    {
                        item.InitializeItem(ability.abilityIcon, ability.abilityParameters);
                        m_activeDescriptionItems.Add(item);
                    }
                }
            });

        }

        public void AssignDisplayMeeple(GameObject _meepleObj)
        {
            if (_meepleObj.IsNull())
            {
                return;
            }

            assignedMeepleObj = _meepleObj;
        }

        public void CleanUp()
        {
            UnAssignDisplayMeeple();
            CleanDescriptions();
        }

        private void CleanDescriptions()
        {
            if (m_activeDescriptionItems.Count == 0)
            {
                return;
            }
            
            m_activeDescriptionItems.ForEach(item =>
            {
                item.Release();
                m_cachedDescriptionItems.Add(item);
            });
            
            m_activeDescriptionItems.Clear();
            
        }
        
        private void UnAssignDisplayMeeple()
        {
            assignedMeepleObj = null;
        }

        public void OnFinishMovement()
        {
            m_isMoving = false;
        }

        private void SelectCard()
        {
            isSelected = !isSelected;
            //Change to selected row
            MeepleItemSelected?.Invoke(assignedData);
        }

        private void HighlightCard(bool _isHighlighted)
        {
            
        }

        #endregion
        
        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            SelectCard();
        }

        public void OnUnselected()
        {
            
        }

        public void OnHover()
        {
            if (isSelected || m_isMoving)
            {
                return;
            }

            var raisedPos = m_inHandPosition + (Vector3.up * 0.25f);
            SetMovement(raisedPos, transform.forward, false);
            
            
            HighlightCard(true);
        }

        public void OnUnHover()
        {
            if (isSelected)
            {
                return;
            }
            
            SetMovement(m_inHandPosition, transform.forward, false);
            HighlightCard(false);
        }

        #endregion
    }
}