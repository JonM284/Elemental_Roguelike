using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Data.AbilityDatas;
using Project.Scripts.Utils;
using Runtime.Character;
using Runtime.Weapons;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Runtime.GameControllers
{
    public class ProjectileController : GameControllerBase
    {

        #region Static

        public static ProjectileController Instance { get; private set; }

        #endregion

        #region Private Fields

        private Transform m_disabledProjectilePool;

        private Transform m_disabledZonePool;
        
        private List<ProjectileBase> m_cachedProjectiles = new List<ProjectileBase>();
        
        private List<ZoneBase> m_cachedZones = new List<ZoneBase>();
        
        #endregion
        
        
        #region Accessors

        public Transform disabledProjectilePool => CommonUtils.GetRequiredComponent(ref m_disabledProjectilePool, () =>
        {
            var t = TransformUtils.CreatePool(transform, false);
            t.RenameTransform("Disabled Projectile Pool");
            return t;
        });
        
        public Transform disabledZonePool => CommonUtils.GetRequiredComponent(ref m_disabledZonePool, () =>
        {
            var t = TransformUtils.CreatePool(transform, false);
            t.RenameTransform("Disabled Zone Pool");
            return t;
        });

        #endregion

        #region Inherited Classes

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        public override void Cleanup()
        {
            m_cachedProjectiles.ForEach(c => Destroy(c.gameObject));
            m_cachedProjectiles.Clear();
            m_cachedZones.ForEach(c => Destroy(c.gameObject));
            m_cachedZones.Clear();
            for (var n = 0; n < disabledProjectilePool.childCount; ++n)
            {
                Transform temp = disabledProjectilePool.GetChild(n);
                GameObject.Destroy(temp.gameObject);
            }
            
            for (var n = 0; n < disabledZonePool.childCount; ++n)
            {
                Transform temp = disabledZonePool.GetChild(n);
                GameObject.Destroy(temp.gameObject);
            }
            base.Cleanup();
        }

        #endregion


        #region Class Implementation

        public void ReturnToPool(ProjectileBase projectile)
        {
            if (projectile.IsNull())
            {
                return;
            }

            m_cachedProjectiles.Add(projectile);
            projectile.transform.ResetTransform(disabledProjectilePool);
        }

        public void ReturnToPool(ZoneBase zone)
        {
            if (zone.IsNull())
            {
                return;
            }
            
            m_cachedZones.Add(zone);
            zone.transform.ResetTransform(disabledZonePool);
        }
        
        public async UniTask GetProjectileAt(ProjectileAbilityData projectileAbilityData, Transform user, 
            Vector3 startPos, Vector3 startRotation, Vector3 endPos, Transform target, CancellationToken cancellationToken)
        {
            if (projectileAbilityData.IsNull())
            {
                Debug.LogError("Projectile Info Null");
                return;
            }
            
            var foundProjectile = 
                m_cachedProjectiles.FirstOrDefault(c
                    => c.projectileAbilityData.abilityGUID == projectileAbilityData.abilityGUID);

            //projectile not found in cachedProjectiles
            if (!foundProjectile.IsNull())
            {
                if (m_cachedProjectiles.Contains(foundProjectile))
                {
                    m_cachedProjectiles.Remove(foundProjectile);
                }

                foundProjectile.transform.parent = null;
                foundProjectile.transform.position = startPos;
                foundProjectile.transform.forward = startRotation;
            
                foundProjectile.Initialize(projectileAbilityData, user ,startPos, endPos, target);
                return;
            }
            
            //instantiate gameobject
            var handle = Addressables.LoadAssetAsync<GameObject>(projectileAbilityData.projectilePrefab);

            await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }
            
            var newProjectileObj = Instantiate(handle.Result);
            
            newProjectileObj.transform.parent = null;
            newProjectileObj.transform.forward = startRotation;
            
            var newProjectile = newProjectileObj.GetComponent<ProjectileBase>();
            
            if (newProjectile.IsNull())
            {
                return;
            }
            
            newProjectile.Initialize(projectileAbilityData, user ,startPos , endPos, target);
        }


        public async UniTask GetZoneAt(AoeZoneData aoeZoneData, Vector3 _spawnPosition, Transform _user, CancellationToken cancellationToken)
        {
            if (aoeZoneData.IsNull())
            {
                Debug.LogError("Zone Info Null");
                return;
            }

            var foundZone = m_cachedZones.FirstOrDefault(zb => zb.MAoeZoneRef == aoeZoneData);

            //projectile found in cachedProjectiles
            if (!foundZone.IsNull())
            {
                if (m_cachedZones.Contains(foundZone))
                {
                    m_cachedZones.Remove(foundZone);
                }

                foundZone.transform.parent = null;
                foundZone.transform.position = _spawnPosition;

                foundZone.Initialize(aoeZoneData, _user);
                return;
            }
            
            //instantiate gameobject
            var handle = Addressables.LoadAssetAsync<GameObject>(aoeZoneData.zonePrefab);
            
            await UniTask.WaitUntil(() => handle.IsDone, cancellationToken: cancellationToken);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                return;
            }
            
            var newZoneObj = Instantiate(handle.Result, _spawnPosition, Quaternion.identity);
            newZoneObj.TryGetComponent(out ZoneBase zoneBase);
            
            if (zoneBase.IsNull())
            {
                return;
            }
            
            zoneBase.Initialize(aoeZoneData, _user);
        }

        #endregion

    }
}