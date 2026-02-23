using System.Threading;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Weapons;

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

            GetTokenSource();
            cts.Token.ThrowIfCancellationRequested();
            
            var endPos = targetTransform != null ? targetTransform.position : targetPosition;

            //ToDo: reset position to hand position
            var projectileObject = await ObjectPoolController.Instance.CreateObjectAsync(
                ObjectPoolController.ProjectilePoolName, projectileAbilityData.abilityGUID,
                projectileAbilityData.projectilePrefab,
                currentOwner.transform.position, cts.Token);

            projectileObject.TryGetComponent(out ProjectileBase projectileBase);

            if (projectileBase.IsNull())
            {
                ObjectPoolController.Instance.ReturnToPool(ObjectPoolController.ProjectilePoolName,
                    projectileAbilityData.abilityGUID,
                    projectileObject).Forget();
                return;
            }

            projectileBase.Initialize(projectileAbilityData, currentOwner.transform,
                currentOwner.transform.position, endPos, targetTransform);
        }

        public override void OnAbilityUsed() { }

        #endregion
        
    }
}