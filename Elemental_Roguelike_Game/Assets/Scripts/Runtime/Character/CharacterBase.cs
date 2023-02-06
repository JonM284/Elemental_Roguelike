using Project.Scripts.Utils;
using Runtime.Selection;
using UnityEngine;
using Utils;

namespace Runtime.Character
{
    [RequireComponent(typeof(CharacterAbilityManager))]
    [RequireComponent(typeof(CharacterLifeManager))]
    [RequireComponent(typeof(CharacterMovement))]
    [RequireComponent(typeof(CharacterRotation))]
    public class CharacterBase: MonoBehaviour, ISelectable
    {

        #region Serialized Fields


        #endregion

        #region Private Fields

        private int m_characterActionPoints = 3;

        private bool m_isActive;

        private bool m_isInBattle;

        private bool m_finishedTurn;
        
        private CharacterAbilityManager m_characterAbilityManager;
        
        private CharacterLifeManager m_characterLifeManager;
        
        private CharacterMovement m_characterMovement;
        
        private CharacterRotation m_characterRotation;
        
        #endregion

        #region Accessors

        public CharacterAbilityManager characterAbilityManager => CommonUtils.GetRequiredComponent(ref m_characterAbilityManager,
            () =>
            {
                var cam = GetComponent<CharacterAbilityManager>();
                return cam;
            });
        
        public CharacterLifeManager characterLifeManager => CommonUtils.GetRequiredComponent(ref m_characterLifeManager,
            () =>
            {
                var cam = GetComponent<CharacterLifeManager>();
                return cam;
            });
        
        public CharacterMovement characterMovement => CommonUtils.GetRequiredComponent(ref m_characterMovement,
            () =>
            {
                var cam = GetComponent<CharacterMovement>();
                return cam;
            });
        
        public CharacterRotation characterRotation => CommonUtils.GetRequiredComponent(ref m_characterRotation,
            () =>
            {
                var cam = GetComponent<CharacterRotation>();
                return cam;
            });

        public bool isAlive => characterLifeManager.isAlive;

        public bool finishedTurn => m_finishedTurn;

        #endregion

        #region Class Implementation

        public void InitializeCharacterBattle()
        {
            if (characterAbilityManager.characterAbilities.Count > 0)
            { 
                characterAbilityManager.characterAbilities.ForEach(a => a.Initialize());
            }
            characterMovement.SetCharacterBattleStatus();
        }

        public void ResetCharacterActions()
        {
            if (!isAlive)
            {
                return;
            }
            
            m_characterActionPoints = 3;
            m_isActive = true;
        }

        #endregion

        #region ISelectable Inherited Methods

        public void OnSelect()
        {
            TurnUtils.SetActiveCharacter(this);
        }

        public void OnUnselected()
        {
            
        }

        #endregion
       
    }
}