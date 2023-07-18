using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.ScriptedAnimations
{
    public abstract class AnimationsBase: MonoBehaviour
    {

        #region Events

        public UnityEvent OnAnimationFinished;

        #endregion
        
        #region Serialized Fields

        [SerializeField] protected float m_maxTime = 1;

        [SerializeField] protected AnimationCurve m_curve = AnimationCurve.Linear(0,0,1,1);

        #endregion

        #region Private Fields

        private float m_startTime;

        private float m_progress;
        
        private bool m_isPlaying;

        private bool m_isPlayingReverse;
        
        #endregion

        #region Accessors

        public bool isPlaying => m_isPlaying;

        #endregion


        #region Class Implementation

        private IEnumerator ApplyAnimation()
        {
            while (m_isPlaying)
            {
                m_progress = (Time.time - m_startTime) / m_maxTime;
                                    
                SetProgress(m_progress);
                
                if (m_isPlaying && m_progress >= 1)
                {
                    OnAnimationFinished?.Invoke();
                    SetProgress(1);
                    m_isPlaying = false;
                    yield break;
                }
                yield return null;
            }
        }

        public void Play()
        {
            m_startTime = Time.time;
            m_isPlaying = true;
            StartCoroutine(ApplyAnimation());
        }

        public virtual void SetProgress(float progress)
        {
            SetAnimationValue(m_curve.Evaluate(progress));
        }

        public abstract void SetAnimationValue(float progress);

        #endregion

    }
}