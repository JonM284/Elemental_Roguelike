using System.Collections;
using Runtime.ScriptedAnimations;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    public class ChestEventManager: GameplayEventBase
    {

        public AnimationListPlayer openChestAnimation;

        [SerializeField] private Transform chestTop;

        public void DoEntireEvent()
        {
            StartCoroutine(C_WaitToEnd());
        }

        public IEnumerator C_WaitToEnd()
        {
            openChestAnimation.Play();
            
            yield return new WaitUntil(() => !openChestAnimation.isPlaying);

            yield return new WaitForSeconds(1f);
            
            OnConfirmEventEnd();
        }

        public void ResetChest()
        {
            chestTop.localRotation = Quaternion.Euler(Vector3.zero);
        }
        
    }
}