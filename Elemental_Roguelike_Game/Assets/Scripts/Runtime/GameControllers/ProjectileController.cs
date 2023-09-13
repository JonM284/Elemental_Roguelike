using System;
using System.Collections.Generic;
using System.Linq;
using Data;
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
        
        public void GetProjectileAt(ProjectileInfo projectileInfo, Transform user ,Vector3 startPos, Vector3 startRotation, Vector3 endPos)
        {
            if (projectileInfo == null)
            {
                Debug.LogError("Projectile Info Null");
                return;
            }
            
            var foundProjectile = m_cachedProjectiles.FirstOrDefault(c => c.m_projectileRef == projectileInfo);

            //projectile not found in cachedProjectiles
            if (foundProjectile == null)
            {
                //instantiate gameobject
                var handle = Addressables.LoadAssetAsync<GameObject>(projectileInfo.projectilePrefab);
                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        var newProjectileObj = Instantiate(handle.Result);
                        newProjectileObj.transform.parent = null;
                        newProjectileObj.transform.forward = startRotation;
                        var newProjectile = newProjectileObj.GetComponent<ProjectileBase>();
                        if (newProjectile != null)
                        {
                            foundProjectile = newProjectile;
                            newProjectile.Initialize(projectileInfo, user ,startPos , endPos);
                        }
                    }
                };
                return;
            }

            if (m_cachedProjectiles.Contains(foundProjectile))
            {
                m_cachedProjectiles.Remove(foundProjectile);
            }

            foundProjectile.transform.parent = null;
            foundProjectile.transform.position = startPos;
            foundProjectile.transform.forward = startRotation;
            
            foundProjectile.Initialize(projectileInfo, user ,startPos, endPos);
        }


        public void GetZoneAt(ZoneInfo _zoneInfo, Vector3 _spawnPosition, Transform _user)
        {
            if (_zoneInfo.IsNull())
            {
                Debug.LogError("Zone Info Null");
                return;
            }

            var foundZone = m_cachedZones.FirstOrDefault(zb => zb.m_zoneRef == _zoneInfo);

            //projectile not found in cachedProjectiles
            if (foundZone.IsNull())
            {
                //instantiate gameobject
                var handle = Addressables.LoadAssetAsync<GameObject>(_zoneInfo.zonePrefab);
                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        var newZoneObj = Instantiate(handle.Result, _spawnPosition, Quaternion.identity);
                        newZoneObj.TryGetComponent(out ZoneBase zoneBase);
                        if (!zoneBase.IsNull())
                        {
                            foundZone = zoneBase;
                            zoneBase.Initialize(_zoneInfo, _user);
                        }
                    }
                };
                return;
            }

            if (m_cachedZones.Contains(foundZone))
            {
                m_cachedZones.Remove(foundZone);
            }

            foundZone.transform.parent = null;
            foundZone.transform.position = _spawnPosition;

            foundZone.Initialize(_zoneInfo, _user);
        }

        #endregion

    }
}