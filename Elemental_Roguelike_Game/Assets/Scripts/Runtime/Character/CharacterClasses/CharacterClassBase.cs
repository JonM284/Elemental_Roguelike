using Data.CharacterData;
using Data.Sides;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Gameplay;
using Runtime.Managers;
using UnityEngine;

namespace Runtime.Character.CharacterClasses
{
    public abstract class CharacterClassBase: MonoBehaviour, IReactor
    {
        

        #region Private Fields
        
        private bool m_isPerformingReaction;
        
        #endregion

        #region Accessors
        
        bool IReactor.isPerformingReaction
        {
            get => m_isPerformingReaction;
            set => m_isPerformingReaction = value;
        }

        #endregion

        #region Class Implementation

        public abstract void UpdateOverwatch();

        #endregion







    }
}