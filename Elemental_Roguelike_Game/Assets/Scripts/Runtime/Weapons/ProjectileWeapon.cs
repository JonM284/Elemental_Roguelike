using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using Data.Elements;
using Project.Scripts.Utils;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public class ProjectileWeapon: WeaponBase
    {

        #region Private Fields

        private AudioSource m_audioSource;

        private ProjectileInfo projectileInfoByElement;
        
        #endregion
        
        #region Accessors

        private ProjectileWeaponData projectileWeaponData => weaponData as ProjectileWeaponData;

        private AudioSource audioSource => CommonUtils.GetRequiredComponent(ref m_audioSource, () =>
        {
            var audioS = GetComponent<AudioSource>();
            return audioS;
        });
        
        #endregion
        
        #region Class Implementation

        public override void Initialize(GameObject _ownerObj, Transform _originTransform, WeaponData _assignedWeaponData, ElementTyping _type)
        {
            base.Initialize(_ownerObj, _originTransform, _assignedWeaponData, _type);
            projectileInfoByElement =
                projectileWeaponData.allPossibleProjectiles.FirstOrDefault(pi =>
                    pi.projectileType == weaponElementType);

            
            if (weaponData.weaponAudio.Count == 0)
            {
                return;
            }
            
            if (!weaponData.hasMultipleWeaponAudios)
            {
                audioSource.clip = weaponData.weaponAudio[0];
            }
        }
        
        

        public override void SelectPosition(Vector3 _inputPosition)
        {
            if (_inputPosition.IsNan())
            {
                return;
            }

            m_targetPosition = _inputPosition;
        }

        public override void SelectTarget(Transform _inputTransform)
        {
            if (_inputTransform == null)
            {
                return; 
            }
            
            m_targetTransform = _inputTransform;
        }

        public override void UseWeapon()
        {
            if (currentOwner == null)
            {
                return;
            }

            StartCoroutine(C_Shoot());
            
            base.UseWeapon();
        }

        private void PlayWeaponAudio()
        {
            if (weaponData.weaponAudio.Count == 0)
            {
                return;
            }
            
            //If there are more than one possible sound for the gun, play a random sound
            //otherwise skip this step
            if (weaponData.hasMultipleWeaponAudios)
            {
                audioSource.clip = weaponData.weaponAudio[Random.Range(0, weaponData.weaponAudio.Count - 1)];
            }
            
            
            audioSource.Play();
        }

        private void PlayVFX()
        {
            if (m_assignedWeaponMuzzleVFX.Count == 0)
            {
                return;
            }
            
            m_assignedWeaponMuzzleVFX.ForEach(ps => ps.Play());
        }

        private IEnumerator C_Shoot()
        {
            var m_endPos = m_targetTransform != null ? m_targetTransform.position : m_targetPosition;
            //direct direction to target
            var dir = m_endPos.FlattenVector3Y() - m_originTransform.position.FlattenVector3Y();
            
            if (projectileWeaponData.isBurstWeapon)
            {
                PlayWeaponAudio();
                PlayVFX();
                //shoot one directly at target
                //projectileInfoByElement.PlayAt(currentOwner.transform ,m_originTransform.transform.position, dir, m_endPos);
                
                for (int i = 0; i < projectileWeaponData.amountOfShots; i++)
                {
                    float spreadX = Random.Range(-1, 1);
                    float spreadY = Random.Range(-1, 1);
                    Vector3 shotSpread = new Vector3(spreadX, spreadY, 0).normalized * projectileWeaponData.coneSize;
                    Vector3 newEndPos = m_endPos + shotSpread;
                   //projectileInfoByElement.PlayAt(currentOwner.transform ,m_originTransform.transform.position, dir, newEndPos);
                }
                yield break;
            }

            if (!projectileWeaponData.isBurstWeapon && projectileWeaponData.hasMultipleBullets)
            {
                //automatic gun
                for (int i = 0; i < projectileWeaponData.amountOfShots; i++)
                {
                    PlayVFX();
                    PlayWeaponAudio();
                    //projectileInfoByElement.PlayAt(currentOwner.transform ,m_originTransform.transform.position, dir, m_endPos);
                    yield return new WaitForSeconds(projectileWeaponData.fireRate);
                }
                
                yield break;
            }
            
            if (!projectileWeaponData.isBurstWeapon && !projectileWeaponData.hasMultipleBullets)
            {
                PlayVFX();
                PlayWeaponAudio();
                //projectileInfoByElement.PlayAt(currentOwner.transform ,m_originTransform.transform.position, dir, m_endPos);
            }

        }

        #endregion
    }
}