using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Utils;
using Runtime.GameControllers;
using Runtime.VFX;
using UnityEngine;
using Utils;

namespace GameControllers
{
    public class VFXController: GameControllerBase
    {

        #region Private Fields

        private Transform m_vfxPool;
        
        private Transform m_enabledVFXPool;

        private List<VFXPlayer> m_cached_VFX = new List<VFXPlayer>();

        private List<VFXPlayer> m_active_VFX = new List<VFXPlayer>();

        #endregion
        
        #region Accessors

        public Transform vfxPool => CommonUtils.GetRequiredComponent(ref m_vfxPool, () =>
        {
            var t = TransformUtils.CreatePool(transform, false);
            return t;
        });

        #endregion

        #region Controller Inherited Fields

        public override void Cleanup()
        {
            m_cached_VFX.ForEach(c => Destroy(c.gameObject));
            m_cached_VFX.Clear();
            m_active_VFX.ForEach(c => Destroy(c.gameObject));
            m_active_VFX.Clear();
            if (m_vfxPool != null && m_vfxPool.childCount > 0)
            {
                for (var n = 0; n < m_vfxPool.childCount; ++n)
                {
                    Transform temp = m_vfxPool.GetChild(n);
                    GameObject.Destroy(temp.gameObject);
                }   
            }
            base.Cleanup();
        }

        #endregion

        #region Unity Events

        void LateUpdate()
        {
            if (m_active_VFX.Count == 0)
            {
                return;
            }
            
            if (m_active_VFX.Count > 0)
            {
                var currentActive = Enumerable.ToList(m_active_VFX);
                currentActive.ForEach(vfx => vfx.CheckVFX());
            }
        }

        #endregion
        
        #region Class Implementation

        /// <summary>
        /// Return incoming vfx to a pool to avoid constant instantiation
        /// </summary>
        /// <param name="vfxPlayer"></param>
        public void ReturnToPool(VFXPlayer vfxPlayer)
        {
            if (vfxPlayer == null)
            {
                return;
            }

            if (m_active_VFX.Contains(vfxPlayer))
            {
                m_active_VFX.Remove(vfxPlayer);
            }
            
            m_cached_VFX.Add(vfxPlayer);
            vfxPlayer.Stop();
            vfxPlayer.transform.ResetTransform(vfxPool);
        }

        public void PlayAt(VFXPlayer vfxPlayer, Vector3 position, Quaternion rotation, Transform activeParent = null)
        {
            if (vfxPlayer.IsNull())
            {
                Debug.Log("No VFX Player Attached");
                return;
            }

            
            var foundVFX = m_cached_VFX.FirstOrDefault(c => c.vfxplayerIdentifier == vfxPlayer.vfxplayerIdentifier);

            if (!foundVFX)
            {
                foundVFX = Instantiate(vfxPlayer);
            }
            else
            {
                m_cached_VFX.Remove(foundVFX);
            }

            foundVFX.transform.parent = !activeParent.IsNull() ? activeParent : null;
            foundVFX.transform.position = position;
            foundVFX.transform.rotation = rotation;

            foundVFX.Play();
            
            m_active_VFX.Add(foundVFX);
        }

        #endregion


    }
}