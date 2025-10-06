using System.Threading;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.GameControllers;

namespace Runtime.Abilities
{
    public class ProjectileAbilityEntity : AbilityEntityBase
    {

        #region Accessors

        private ProjectileAbilityData projectileAbilityData => abilityData as ProjectileAbilityData;

        #endregion
        
        #region Ability Inherited Methods
        
        protected override async UniTask PerformAbilityAction()
        {
            if (!cts.IsNull())
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new CancellationTokenSource();
            
            var m_endPos = targetTransform != null ? targetTransform.position : targetPosition;
            ProjectileController.Instance.GetProjectileAt(projectileAbilityData, currentOwner.transform,
                currentOwner.transform.position, currentOwner.transform.forward, m_endPos, targetTransform,cts.Token);
        }

        public override void OnAbilityUsed() { }

        #endregion
        
    }
}