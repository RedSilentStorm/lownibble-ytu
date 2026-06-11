using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Boot, Fight, Comic, Transition }
    public GameState currentState = GameState.Boot;
    // Optional config passed from other systems (e.g. Comic) to customize next Fight scene
    public string pendingFightConfig = null;

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
        // Oyun Comic ile başlar; comic bitince ComicManager Fight'a geçirir
        SceneLoader.Instance.LoadAdditiveScene("Comic", () =>
        {
            currentState = GameState.Comic;
            Debug.Log("Comic moduna girildi.");
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

    // Overload that accepts an optional config string which will be available
    // to the Fight scene after it's loaded (see FightManager.ApplyFightConfig).
    public void SwitchToFight(string config)
    {
        pendingFightConfig = config;
        SwitchToFight();
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

                // If a pending config was provided before loading, try to apply it.
                if (!string.IsNullOrEmpty(pendingFightConfig))
                {
                    var fm = FindObjectOfType<FightManager>();
                    if (fm != null)
                    {
                        fm.ApplyFightConfig(pendingFightConfig);
                        pendingFightConfig = null;
                    }
                    else
                    {
                        Debug.LogWarning("FightManager bulunamadı - pendingFightConfig uygulanamadı.");
                    }
                }
            });
        });
    }
}