using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Character;
using Runtime.GameControllers;
using Runtime.ScriptedAnimations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI.DataReceivers
{
    public class JuiceUIDataModel: MonoBehaviour
    {
        
        #region Serialized Field

        [SerializeField] private UIWindowDialog uiWindow;
        
        [Space(20)]
        [Header("Reaction Related UI")]
        [SerializeField] private GameObject reactionUI;

        [SerializeField] private float textMaxTime = 1.5f;
        
        [SerializeField] private TMP_Text leftCharacterText;

        [SerializeField] private TMP_Text rightCharacterText;

        [SerializeField] private TMP_Text leftDescriptionText;
        
        [SerializeField] private TMP_Text rightDescriptionText;

        [SerializeField] private TMP_Text reactionDescriptionText;

        [SerializeField] private List<RectTransform> sideParents = new List<RectTransform>();

        [SerializeField] private Color resetColor;

        [SerializeField] private List<Image> fades = new List<Image>();

        [SerializeField] private AnimationListPlayer reactionStartedAnimation;

        [SerializeField] private AnimationListPlayer leftWinAnimation;

        [SerializeField] private AnimationListPlayer rightWinAnimation;

        [SerializeField] private AnimationListPlayer reactionEndedAnimation;

        [Space]
        [Header("Goal Related UI")] 
        [SerializeField] private GameObject goalScoreUI;
        
        [SerializeField] private AnimationListPlayer goalScoredPlayerAnimation;
        
        [SerializeField] private AnimationListPlayer goalScoredEnemyAnimation;

        [Space]
        [Header("Change Sides UI")] 
        
        [SerializeField] private GameObject turnUI;
        
        [SerializeField] private AnimationListPlayer playerSideInAnimation;
        
        [SerializeField] private AnimationListPlayer enemySideInAnimation;
        
        [Space] 
        [Header("Death Related UI")] 
        
        [SerializeField] private GameObject deathUI;
        
        [SerializeField] private Image deathImage;

        [SerializeField] private Color deathResetColor;
        
        [SerializeField] private AnimationListPlayer deathStartedAnimation;
        
        [SerializeField] private AnimationListPlayer deathEndedAnimation;
        
        [SerializeField] private AnimationListPlayer deathMiddleAnimation;

        [Header("Common")]
        [SerializeField] private List<RawImage> tex1Images;
        
        [SerializeField] private List<RawImage> tex2Images;
        
        [SerializeField] private List<RawImage> tex3Images;
        
        #endregion

        #region Private Fields

        private int m_endValueL;

        private int m_endValueR;

        private float m_currentValueL;

        private float m_currentValueR;

        private float m_startTime;

        private bool m_valuesChanged;

        #endregion

        #region Unity Events

        private void Start()
        {
            SetImageTextureReferences();
        }

        private void OnEnable()
        {
            TurnController.OnBattleEnded += TurnControllerOnOnBattleEnded;
        }

        private void OnDisable()
        {
            TurnController.OnBattleEnded -= TurnControllerOnOnBattleEnded;
        }

        #endregion

        #region Class Implementation

        private void TurnControllerOnOnBattleEnded()
        {
            uiWindow.Close();
        }

        private void SetImageTextureReferences()
        {
            var tex1 = JuiceController.Instance.GetTexture1();
            var tex2 = JuiceController.Instance.GetTexture2();
            var tex3 = JuiceController.Instance.GetTexture3();
            
            tex1Images.ForEach(i => i.texture = tex1);
            tex2Images.ForEach(i => i.texture = tex2);
            tex3Images.ForEach(i => i.texture = tex3);
        }
        
        public IEnumerator C_ReactionEvent(int _endValueL, int _endValueR, Action callback, CharacterClass _characterClass, bool _isLeftReactor)
        {
            reactionUI.SetActive(true);

            m_currentValueL = 0;
            m_currentValueR = 0;
            
            leftCharacterText.text = "0";
            rightCharacterText.text = "0";

            reactionDescriptionText.text = $"{_characterClass.ToString()} Reaction";

            leftDescriptionText.text = _isLeftReactor ? "Reactor" : "Target";
            rightDescriptionText.text = _isLeftReactor ? "Target" : "Reactor";

            sideParents.ForEach(rt => rt.localScale = Vector3.one);
            
            ResetFades();

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
            
            callback?.Invoke();
            
            m_valuesChanged = false;
            
            Debug.Log("Reaction animation ended");

        }

        public IEnumerator C_CountUpToValue()
        {
            var percentage = 0f;
            Debug.Log($"<color=orange>Started Count Up</color>");
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
            
            if (m_currentValueL > m_currentValueR)
            {
                leftWinAnimation.Play();
            }else if (m_currentValueR > m_currentValueL)
            {
                rightWinAnimation.Play();
            }
            Debug.Log($"<color=orange>Started Loss Win Animation</color>");
            yield return new WaitUntil(() => !leftWinAnimation.isPlaying && !rightWinAnimation.isPlaying);

        }

        public IEnumerator C_DeathUIEvent()
        {
            deathUI.SetActive(true);
            
            deathImage.color = deathResetColor;
            
            ResetFades();
            
            deathStartedAnimation.Play();

            yield return new WaitUntil(() => !deathStartedAnimation.isPlaying);
            
            deathMiddleAnimation.Play();

            yield return new WaitUntil(() => !deathMiddleAnimation.isPlaying);
            
            deathEndedAnimation.Play();

            yield return new WaitUntil(() => !deathEndedAnimation.isPlaying);
            
            deathUI.SetActive(false);
        }

        public IEnumerator C_ChangeSide(bool _isPlayerTurn)
        {
            turnUI.SetActive(true);

            if (_isPlayerTurn)
            {
                playerSideInAnimation.Play();
            }
            else
            {
                enemySideInAnimation.Play();
            }

            yield return new WaitUntil(() => !playerSideInAnimation.isPlaying && !enemySideInAnimation.isPlaying);


            turnUI.SetActive(false);
        }

        public IEnumerator C_ScoreGoal(bool _isPlayerGoal)
        {
            
            goalScoreUI.SetActive(true);
            
            if (_isPlayerGoal)
            {
                goalScoredEnemyAnimation.Play();
            }
            else
            {
                goalScoredPlayerAnimation.Play();   
            }
            
            yield return new WaitUntil(() => !goalScoredPlayerAnimation.isPlaying && !goalScoredEnemyAnimation.isPlaying);
            
            goalScoreUI.SetActive(false);
            
        }

        private void ResetFades()
        {
            fades.ForEach(im => im.color = resetColor);
        }

        #endregion


    }
}