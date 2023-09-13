using System;
using System.Collections;
using Data;
using Project.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Runtime.GameControllers
{
    public class SceneController : GameControllerBase
    {

        #region Instance

        public static SceneController Instance { get; private set; }

        #endregion

        #region Actions

        public static event Action<SceneName, bool> OnLevelPrefinishedLoading;

        public static event Action<SceneName, bool> OnLevelFinishedLoading;

        //Additive Scenes
        public static event Action<SceneName, bool> OnAdditiveScenePrefinishedLoading;

        public static event Action<SceneName, bool> OnAdditiveSceneFinishedLoading;

        public static event Action<SceneName, bool> OnAdditiveScenePrefinishedUnloading;
        
        public static event Action<SceneName, bool> OnAdditiveSceneFinishedUnloading;
        
        #endregion

        #region Private Fields

        private bool m_hasTriggeredPreloadAction;

        private bool m_hasTriggeredLoadAction;

        private bool m_hasTriggeredUnloadAction;
        
        private bool m_hasTriggeredPreunloadAction;
        
        

        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            if (!Instance.IsNull())
            {
                return;
            }
            
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public void LoadScene(SceneName _targetScene, bool _isMatchScene)
        {
            ResetBools();
            Debug.Log($"LOADING SCENE: {_targetScene.ToString()}");
            StartCoroutine(C_LoadSceneAsync(_targetScene, _isMatchScene));
        }

        public void LoadSceneAdditive(SceneName _targetScene, bool _isMatchScene)
        {
            ResetBools();
            StartCoroutine(LoadAdditiveSceneAsync(_targetScene, _isMatchScene));
        }

        public void UnloadSceneAdditive(SceneName _targetScene, bool _isMatchScene)
        {
            ResetBools();
            StartCoroutine(C_UnloadAdditiveSceneAsync(_targetScene, _isMatchScene));
        }

        private void ResetBools()
        {
            m_hasTriggeredLoadAction = false;
            m_hasTriggeredPreloadAction = false;
            m_hasTriggeredUnloadAction = false;
            m_hasTriggeredPreunloadAction = false;
        }

        private IEnumerator C_LoadSceneAsync(SceneName _targetScene, bool _isMatchScene)
        {
            yield return null;
            
            Debug.Log("LOADING SCENE ASYNC START C");
            
            UIUtils.FadeBlack(true);

            yield return new WaitForSeconds(1f);
            
            var sceneLoad = SceneManager.LoadSceneAsync(_targetScene.ToString(), LoadSceneMode.Single);

            while (!sceneLoad.isDone)
            {
                yield return null;
                if (sceneLoad.progress >= 0.9 && !m_hasTriggeredPreloadAction)
                {
                    m_hasTriggeredPreloadAction = true;
                    OnLevelPrefinishedLoading?.Invoke(_targetScene, _isMatchScene);
                }
            }
            
            if (!m_hasTriggeredLoadAction)
            {
                Debug.Log("TRIGGERING LEVEL FINISH ACTION");
                m_hasTriggeredLoadAction = true;
                OnLevelFinishedLoading?.Invoke(_targetScene, _isMatchScene);
            }
            
            Debug.Log("LOADING SCENE ASYNC END C");

            yield return null;

        }

        private IEnumerator LoadAdditiveSceneAsync(SceneName _targetScene, bool _isMatchScene)
        {
            yield return null;

            var sceneLoad = SceneManager.LoadSceneAsync(_targetScene.ToString(), LoadSceneMode.Additive);

            while (!sceneLoad.isDone)
            {
                yield return null;

                if (sceneLoad.progress >= 0.9 && !m_hasTriggeredPreloadAction)
                {
                    m_hasTriggeredPreloadAction = true;
                    OnAdditiveScenePrefinishedLoading?.Invoke(_targetScene, _isMatchScene);
                }
                
            }
            
            if (!m_hasTriggeredLoadAction)
            {
                m_hasTriggeredLoadAction = true;
                OnAdditiveSceneFinishedLoading?.Invoke(_targetScene, _isMatchScene);
            }
            
            yield return null;
            
        }

        private IEnumerator C_UnloadAdditiveSceneAsync(SceneName _targetScene, bool _isMatchScene)
        {
            yield return null;

            var sceneUnload = SceneManager.UnloadSceneAsync(_targetScene.ToString());

            while (!sceneUnload.isDone)
            {
                yield return null;

                if (sceneUnload.progress >= 0.9 && !m_hasTriggeredPreunloadAction)
                {
                    m_hasTriggeredPreunloadAction = true;
                    OnAdditiveScenePrefinishedUnloading?.Invoke(_targetScene, _isMatchScene);
                }
            }
            
            if (!m_hasTriggeredUnloadAction)
            {
                m_hasTriggeredUnloadAction = true;
                OnAdditiveSceneFinishedUnloading?.Invoke(_targetScene, _isMatchScene);
            }
            
            yield return null;
            
        }

        #endregion


    }
}