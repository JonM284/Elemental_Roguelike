using System;
using Data;
using Project.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        }


        public void OnSelectItem()
        {
            Debug.Log("Selected");
            
            isSelected = !isSelected;

            highlightImage.SetActive(isSelected);
            
            //Change to selected row
            MeepleItemSelected?.Invoke(assignedData);
            
        }

        #endregion
        
        
    }
}