using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadAdditiveScene(string sceneName, Action onComplete = null)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, onComplete));
    }

    public void UnloadAdditiveScene(string sceneName, Action onComplete = null)
    {
        StartCoroutine(UnloadSceneRoutine(sceneName, onComplete));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, Action onComplete)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        onComplete?.Invoke();
    }

    private IEnumerator UnloadSceneRoutine(string sceneName, Action onComplete)
    {
        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone)
            yield return null;

        onComplete?.Invoke();
    }
}