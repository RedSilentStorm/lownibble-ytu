using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ComicManager : MonoBehaviour
{
    public static ComicManager Instance { get; private set; }

    [SerializeField] private PlayableDirector director;
    [SerializeField] private TimelineAsset[] pages; // Tüm sayfa timeline'ları
    private int currentPageIndex = 0;

    private void Awake()
    {
        Instance = this;
        director.stopped += OnPageFinished;
    }

    void Start()
    {
        if (pages.Length > 0)
            PlayPage(currentPageIndex);
        else
            Debug.LogError("Hiç sayfa atanmamış!");
    }

    void PlayPage(int index)
    {
        if (index >= pages.Length)
        {
            // Tüm sayfalar bitti, fight'a dön
            GameManager.Instance.SwitchToFight();
            return;
        }

        director.Play(pages[index]);
    }

    // Signal Receiver bağlantısı için public metot
    public void NextPage()
    {
        director.Stop();
        currentPageIndex++;
        PlayPage(currentPageIndex);
    }

    // Alternatif: timeline durunca otomatik geçiş
    private void OnPageFinished(PlayableDirector pd)
    {
        // Eğer Signal kullanmıyorsanız buradan çağırın
        // NextPage();
    }
}