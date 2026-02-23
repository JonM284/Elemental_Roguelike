using System.Threading;
using Cysharp.Threading.Tasks;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.Weapons;
using UnityEngine;

namespace Runtime.Abilities
{
    public class AoeZoneAbilityEntity: AbilityEntityBase
    {

        #region Accessors

        private AoeZoneAbilityData zoneAbilityData => abilityData as AoeZoneAbilityData;

        #endregion

        public override void ShowAttackIndicator(bool _isActive)
        {
            base.ShowAttackIndicator(_isActive);

            if (_isActive && IsEffectAreaAbility())
                abilityPositionIndicator.transform.localScale =
                    Vector3.one * (zoneAbilityData.zoneRadius * 2f);
        }
        
        

        
        protected override async UniTask PerformAbilityAction()
        {
            Debug.Log("[Zone Ability][Perform Ability Action] ");
            GetTokenSource();
            cts.Token.ThrowIfCancellationRequested();
            
            var endPos = !targetTransform.IsNull() ? targetTransform.position.FlattenVector3Y() : targetPosition;
            
            var zoneBase = await CreateZoneAsync();
            
            //Get zone projectile
            if (zoneAbilityData.displayedProjectile.IsNull())
            {
                zoneBase.OnCreate();
                return;
            }

            Debug.Log("[Zone Ability][Use] Projectile Created");
                
            //ToDo: reset position to hand position
            var projectileObject = await ObjectPoolController.Instance.CreateObjectAsync(
                ObjectPoolController.ProjectilePoolName, zoneAbilityData.abilityGUID,
                zoneAbilityData.displayedProjectile,
                currentOwner.transform.position, cts.Token);

            projectileObject.TryGetComponent(out ProjectileBase projectileBase);

            if (projectileBase.IsNull())
            {
                ObjectPoolController.Instance.ReturnToPool(ObjectPoolController.ProjectilePoolName,
                    zoneAbilityData.abilityGUID,
                    projectileObject, cts.Token).Forget();
                return;
            }

            //Create Display Projectile
            projectileBase.Initialize(currentOwner.transform.position, 
                endPos, zoneAbilityData.projectileTravelTime, zoneAbilityData.projectileYCurve
                , zoneBase.OnCreate);

            Debug.Log("[Zone Ability][Use] Projectile Ended");
            
        }

        private void CreateZone()
        {
            CreateZoneAsync().Forget();
        }

        private async UniTask<ZoneBase> CreateZoneAsync()
        {
            GetTokenSource();
            cts.Token.ThrowIfCancellationRequested();
            
            var endPos = !targetTransform.IsNull() ? targetTransform.position.FlattenVector3Y() : targetPosition;
            
            var zoneObject = await ObjectPoolController.Instance.CreateObjectAsync(
                ObjectPoolController.ZonePoolName, zoneAbilityData.abilityGUID,
                zoneAbilityData.zonePrefab,
                endPos, cts.Token);

            zoneObject.TryGetComponent(out ZoneBase zoneBase);

            if (zoneBase.IsNull())
            {
                ObjectPoolController.Instance.ReturnToPool(ObjectPoolController.ZonePoolName,
                    zoneAbilityData.abilityGUID,
                    zoneObject, cts.Token).Forget();
                Debug.Log("<color=orange>[Zone Ability][Use][ERROR] Did not get zone component.</color>");
                return default;
            }
            
            zoneBase.Initialize(zoneAbilityData, currentOwner);
            Debug.Log("<color=orange>[Zone Ability][Use] Zone Created and Initialized</color>");
            return zoneBase;
        }
        
    }
}