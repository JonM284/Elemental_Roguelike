using Cysharp.Threading.Tasks;
using Data.CharacterData;
using Project.Scripts.Utils;
using Runtime.Character.AI;
using Runtime.Character.StateMachines;
using Runtime.GameControllers;

namespace Runtime.Character
{
    public class EnemyCharacterRegular: CharacterBase
    {

        #region CharacterBase Inherited Methods

        /// <summary>
        /// Initialize all components of character
        /// Also, check for upgrade items and such, then apply here
        /// </summary>
        public override async UniTask InitializeCharacter(CharacterStatsBase _characterStats)
        {
            base.InitializeCharacter(_characterStats);
            
            TryGetComponent(out EnemyAIBase _enemyAI);

            if (!_enemyAI.IsNull())
            {
               _enemyAI.SetupBehaviorTrees();
            }

        }

        public override float GetBaseSpeed()
        {
            if (m_characterStatsBase == null)
            {
                return 0;
            }

            return m_characterStatsBase.baseSpeed;
        }


        protected override void OnBattleEnded()
        {
            //Undecided
        }

        #endregion
    }
}