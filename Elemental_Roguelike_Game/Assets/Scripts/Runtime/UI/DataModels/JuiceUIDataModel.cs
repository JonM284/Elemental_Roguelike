using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using Runtime.ScriptedAnimations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.UI.DataReceivers
{
    public class JuiceUIDataModel: MonoBehaviour
    {
        
        #region Serialized Field

        [SerializeField] private float textMaxTime = 1.5f;


        [Space(20)]
        [Header("Reaction Related UI")]
        [SerializeField] private GameObject reactionUI;

        [SerializeField] private TMP_Text leftCharacterText;

        [SerializeField] private TMP_Text rightCharacterText;

        [SerializeField] private UnityEngine.Camera leftCharCam;

        [SerializeField] private UnityEngine.Camera rightCharCam;
        
        [SerializeField] private AnimationListPlayer reactionStartedAnimation;

        [SerializeField] private AnimationListPlayer reactionEndedAnimation;

        [Space]
        [Header("Goal Related UI")]
        [SerializeField] private AnimationListPlayer goalScoredAnimation;

        #endregion

        #region Private Fields

        private int m_endValueL;

        private int m_endValueR;

        private float m_currentValueL;

        private float m_currentValueR;

        private float m_startTime;

        private bool m_valuesChanged;

        #endregion

        #region Class Implementation
        
        public IEnumerator C_ReactionEvent(int _endValueL, int _endValueR)
        {
            //ToDo: setup characters, numbers, and stats used
            reactionUI.SetActive(true);
            
            //ToDo: Move this to look at the characters that are performing the action
            //rightCharCam.gameObject.SetActive(true);
            //leftCharCam.gameObject.SetActive(true);

            m_currentValueL = 0;
            m_currentValueR = 0;
            
            leftCharacterText.text = "0";
            rightCharacterText.text = "0";

            m_endValueL = _endValueL;
            m_endValueR = _endValueR;
            
            m_startTime = Time.time;
            
            reactionStartedAnimation.Play();

            yield return new WaitUntil(() => !reactionStartedAnimation.isPlaying);

            yield return StartCoroutine(C_CountUpToValue());

            yield return new WaitForSeconds(0.8f);

            reactionEndedAnimation.Play();

            yield return new WaitUntil(() => !reactionEndedAnimation.isPlaying);
            
            reactionUI.SetActive(false);

            m_valuesChanged = false;
            
            Debug.Log("Reaction animation ended");

        }

        public IEnumerator C_CountUpToValue()
        {
            var percentage = 0f;
            while (percentage < 0.98f)
            {
                percentage = (Time.time - m_startTime) / textMaxTime;
                
                m_currentValueL = m_endValueL * percentage;
                leftCharacterText.text = $"{Mathf.FloorToInt(m_currentValueL)}";
                
                m_currentValueR = m_endValueR * percentage;
                rightCharacterText.text = $"{Mathf.FloorToInt(m_currentValueR)}";
                
                if (percentage >= 0.98f)
                {
                    break;
                }

                yield return null;

            }
        }

        #endregion


    }
}