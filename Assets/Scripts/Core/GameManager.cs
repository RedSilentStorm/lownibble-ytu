using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Boot, Fight, Comic, Transition }
    public GameState currentState = GameState.Boot;

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

    private void Start()
    {
        // Oyun başlar başlamaz Fight sahnesini yükle
        SceneLoader.Instance.LoadAdditiveScene("Fight", () =>
        {
            currentState = GameState.Fight;
            Debug.Log("Fight moduna girildi.");
        });
    }

    public void SwitchToComic()
    {
        if (currentState != GameState.Fight) return;
        currentState = GameState.Transition;

        SceneLoader.Instance.UnloadAdditiveScene("Fight", () =>
        {
            SceneLoader.Instance.LoadAdditiveScene("Comic", () =>
            {
                currentState = GameState.Comic;
                Debug.Log("Comic moduna girildi.");
            });
        });
    }

    public void SwitchToFight()
    {
        if (currentState != GameState.Comic) return;
        currentState = GameState.Transition;

        SceneLoader.Instance.UnloadAdditiveScene("Comic", () =>
        {
            SceneLoader.Instance.LoadAdditiveScene("Fight", () =>
            {
                currentState = GameState.Fight;
                Debug.Log("Fight moduna geri dönüldü.");
            });
        });
    }
}