using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.UI
{
    public class SceneController : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreen;
        private const string MainMenuScene = "MainMenu";
        private const string GamePlayScene = "GameplayScene";

        private void Awake()
        {
            var loadingScreen = Instantiate(_loadingScreen);
            loadingScreen.SetActive(false);
        }

        public void LoadMainMenu()
        {
            StartCoroutine(LoadSceneAsync(MainMenuScene));
        }

        public void LoadGameplay()
        {
            StartCoroutine(LoadSceneAsync(GamePlayScene));
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            _loadingScreen.SetActive(true);
            
            while (!operation.isDone)
            {
                yield return null;
            }
        }
    }
}