using Cysharp.Threading.Tasks;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Character.StateMachines;
using Runtime.GameControllers;

namespace Runtime.Character
{
    public class PlayableCharacter: CharacterBase
    {

        #region CharacterBase Inherited Methods
        
        /// <summary>
        /// Initialize all components of character
        /// Also, check for upgrade items and such, then apply here
        /// </summary>
        public override async UniTask InitializeCharacter(CharacterStatsBase _characterStats)
        {
            base.InitializeCharacter(_characterStats);
        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsBase == null)
            {
                return 0;
            }

            return m_characterStatsBase.baseSpeed;
        }

        #endregion

        #region Class Implementation

        protected override void OnBattleEnded()
        {
            //Undecided
        }
        
        
        #endregion


    }
}