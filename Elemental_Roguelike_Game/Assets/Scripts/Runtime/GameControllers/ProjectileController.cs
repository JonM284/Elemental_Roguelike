using System.Collections.Generic;
using System.Linq;
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

        #region Private Fields

        private Transform m_disabledProjectilePool;

        private Transform m_activeProjectilePool;
        
        private List<ProjectileBase> m_cachedProjectiles = new List<ProjectileBase>();

        private List<ProjectileBase> m_activeProjectiles = new List<ProjectileBase>();

        #endregion
        
        
        #region Accessors

        public Transform disabledProjectilePool => CommonUtils.GetRequiredComponent(ref m_disabledProjectilePool, () =>
        {
            var t = TransformUtils.CreatePool(transform, false);
            return t;
        });
        
        public Transform activeProjectilePool => CommonUtils.GetRequiredComponent(ref m_activeProjectilePool, () =>
        {
            var t = TransformUtils.CreatePool(transform, true);
            return t;
        });

        #endregion

        #region Inherited Classes

        public override void Cleanup()
        {
            m_cachedProjectiles.ForEach(c => Destroy(c.gameObject));
            m_cachedProjectiles.Clear();
            m_cachedProjectiles.ForEach(c => Destroy(c.gameObject));
            m_cachedProjectiles.Clear();
            for (var n = 0; n < disabledProjectilePool.childCount; ++n)
            {
                Transform temp = disabledProjectilePool.GetChild(n);
                GameObject.Destroy(temp.gameObject);
            }
            base.Cleanup();
        }

        #endregion


        #region Class Implementation
        
        
        
        public void ReturnToPool(ProjectileBase projectile)
        {
            if (projectile == null)
            {
                return;
            }

            if (m_activeProjectiles.Contains(projectile))
            {
                m_activeProjectiles.Remove(projectile);
            }
            
            m_cachedProjectiles.Add(projectile);
            projectile.transform.ResetTransform(disabledProjectilePool);
        }

        public void GetProjectileAt(ProjectileBase projectileBase, Vector3 startPos, Vector3 startRotation, Vector3 endPos)
        {
            if (projectileBase == null)
            {
                return;
            }
            
            var foundProjectile = m_cachedProjectiles.FirstOrDefault(c => c.projectileRef == projectileBase.projectileRef);

            if (!foundProjectile)
            {
                //instantiate gameobject
                var handle = Addressables.LoadAssetAsync<GameObject>(projectileBase.projectileRef.projectileAsset);
                handle.Completed += operation =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        var newProjectileObj = handle.Result.Clone(activeProjectilePool);
                        var newProjectile = newProjectileObj.GetComponent<ProjectileBase>();
                        if (newProjectile != null)
                        {
                            foundProjectile = newProjectile;
                            newProjectile.Initialize(startPos);
                        }
                    }
                };
            }
            else
            {
                m_cachedProjectiles.Remove(foundProjectile);
            }

            foundProjectile.transform.parent = activeProjectilePool;
            foundProjectile.transform.position = startPos;
            foundProjectile.transform.forward = startRotation;
            
            m_activeProjectiles.Add(foundProjectile);
            foundProjectile.Initialize(startPos);
        }

        #endregion
        
    }
}