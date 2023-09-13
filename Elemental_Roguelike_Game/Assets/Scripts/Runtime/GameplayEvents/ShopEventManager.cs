using System.Collections;
using Runtime.ScriptedAnimations;
using UnityEngine;

namespace Runtime.GameplayEvents
{
    public class ShopEventManager: GameplayEventBase
    {

        #region Serialized Fields

        [SerializeField] private AnimationListPlayer gatchaAnimation;

        [SerializeField] private Transform capsuleTop;

        [SerializeField] private Transform capsule;

        #endregion

        #region Private Fields

        private bool m_hasPresentedCapsule;
        
        private bool m_playerHasInteracted;

        #endregion

        #region Class Implementation
        
        //ToDo: Get rid of this event when the user finishes interaction

        public void ConfirmPress()
        {
            StartCoroutine(C_EntireEvent());
        }

        public IEnumerator C_EntireEvent()
        {
            gatchaAnimation.Play();

            yield return new WaitUntil(() => !gatchaAnimation.isPlaying);

            yield return new WaitForSeconds(0.5f);
            
            OnConfirmEventEnd();
        }

        public void ResetCapsule()
        {
            capsuleTop.localRotation = Quaternion.Euler(Vector3.zero);
            capsule.localPosition = Vector3.zero;
        }

        #endregion



    }
}