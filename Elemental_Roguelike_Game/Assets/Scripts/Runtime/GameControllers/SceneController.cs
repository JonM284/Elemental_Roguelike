using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Runtime.GameControllers
{
    public class SceneController : GameControllerBase
    {

        #region Instance

        public static SceneController Instance { get; private set; }

        #endregion

        #region Actions

        public static event Action<SceneName> OnLevelPrefinishedLoading;

        public static event Action<SceneName> OnLevelFinishedLoading;

        //Additive Scenes
        public static event Action<SceneName> OnAdditiveScenePrefinishedLoading;

        public static event Action<SceneName> OnAdditiveSceneFinishedLoading;
        
        #endregion

        #region GameControllerBase Inherited Methods

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
        }

        #endregion

        #region Class Implementation

        public void LoadScene(SceneName _targetScene)
        {
            StartCoroutine(LoadSceneAsync(_targetScene));
        }
        
        public void LoadSceneAdditive(SceneName _targetScene)
        {
            StartCoroutine(LoadAdditiveSceneAsync(_targetScene));
        }

        private IEnumerator LoadSceneAsync(SceneName _targetScene)
        {
            yield return null;

            var sceneLoad = SceneManager.LoadSceneAsync(_targetScene.ToString(), LoadSceneMode.Single);

            while (!sceneLoad.isDone)
            {

                if (sceneLoad.progress >= 0.9)
                {
                    
                    OnLevelPrefinishedLoading?.Invoke(_targetScene);
                    
                }
                
            }
            
            OnLevelFinishedLoading?.Invoke(_targetScene);

            yield return null;

        }

        private IEnumerator LoadAdditiveSceneAsync(SceneName _targetScene)
        {
            yield return null;

            var sceneLoad = SceneManager.LoadSceneAsync(_targetScene.ToString(), LoadSceneMode.Additive);

            while (!sceneLoad.isDone)
            {

                if (sceneLoad.progress >= 0.9)
                {
                    
                    OnAdditiveScenePrefinishedLoading?.Invoke(_targetScene);
                    
                }
                
            }
            
            OnAdditiveSceneFinishedLoading?.Invoke(_targetScene);

        }

        #endregion


    }
}