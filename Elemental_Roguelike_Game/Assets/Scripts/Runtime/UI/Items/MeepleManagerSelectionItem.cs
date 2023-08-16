using System;
using Data;
using Data.CharacterData;
using Project.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Runtime.UI
{
    public class MeepleManagerSelectionItem : MonoBehaviour
    {

        #region Actions
        
        public static event Action<CharacterStatsData> MeepleItemSelected;

        #endregion

        #region SerializedFields

        [SerializeField] private GameObject highlightImage;

        [SerializeField] private TMP_Text shieldText;

        [SerializeField] private TMP_Text healthText;

        [SerializeField] private TMP_Text abilityDescriptionText;

        [SerializeField] private Image elementIcon;

        #endregion

        #region Private Fields

        private Button m_button;

        #endregion

        #region Accessor

        private Button button => CommonUtils.GetRequiredComponent(ref m_button, () =>
        {
            var b = GetComponent<Button>();
            return b;
        });


        public bool isSelected { get; private set; }


        public CharacterStatsData assignedData { get; private set; }

        #endregion

        #region Class Implementation

        public void InitializeSelectionItem(CharacterStatsData meeple)
        {
            assignedData = meeple;
            
            shieldText.text = $"{meeple.baseShields}";
            
            healthText.text = $"{meeple.baseHealth}";

            if (meeple.abilityReferences.Count > 0)
            {
                string allAbilitiesString = String.Empty;
            
                meeple.abilityReferences.ForEach(s =>
                {
                    //var ability = AbilityUtils.GetAbilityByGUID(s);
                    //string formatString = string.Format($"{ability.abilityName}: {ability.abilityDescription} \n", ability.range, ability.roundCooldownTimer);
                    //allAbilitiesString += formatString;
                });

                abilityDescriptionText.text = allAbilitiesString;    
            }
            
            var _icon = ElementUtils.GetElementTypeByGUID(meeple.meepleElementTypeRef).elementSprite;
            if (_icon.IsNull())
            {
                return;
            }

            elementIcon.sprite = _icon;
        }


        public void OnSelectItem()
        {
            isSelected = !isSelected;

            highlightImage.SetActive(isSelected);
            
            //Change to selected row
            MeepleItemSelected?.Invoke(assignedData);
        }

        #endregion
        
        
    }
}