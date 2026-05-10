using UnityEngine;
using System.Collections;

/// <summary>
/// Merkezi yönetici: Animator-based comic pages yönetir
/// Central Manager: Manages Animator-based comic pages
/// 
/// Sorumlulukları:
/// - Sayfa değiştirme (page switching)
/// - Animator Controller atama (controller assignment)
/// - Sayfa yaşam döngüsü (page lifecycle)
/// 
/// Responsibilities:
/// - Switch between pages
/// - Assign correct Animator Controller
/// - Manage page lifecycle
/// </summary>
public class ComicManager : MonoBehaviour
{
    public static ComicManager Instance { get; private set; }

    [Header("Kamera Animator / Camera Animator")]
    [SerializeField] private Animator cameraAnimator;

    [Header("Sayfa Ayarları / Page Settings")]
    [SerializeField] private ComicPage[] pagesPrefabs; // Tüm comic sayfa prefab'ları
    [SerializeField] private Transform canvasTransform; // Canvas (sayfaları spawn etmek için)

    private int currentPageIndex = 0;
    private ComicPage currentPage;
    private GameObject currentPageInstance;
    private bool isWaitingForNextInput = false; // Animasyon bitince tıklama bekle
    private string pendingNextAction = ""; // Sonraki aksiyon bekleme listesi

    private void Awake()
    {
        // Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (pagesPrefabs.Length > 0)
        {
            LoadPage(0);
        }
        else
        {
            Debug.LogError("[ComicManager] Hiç sayfa prefab'ı atanmamış! Lütfen Inspector'da pagesPrefabs array'ını doldurun.");
        }
    }

    /// <summary>
    /// Sayfa yükle ve Animator Controller'ı değiştir
    /// Load page and switch Animator Controller
    /// </summary>
    public void LoadPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= pagesPrefabs.Length)
        {
            Debug.LogWarning($"[ComicManager] Geçersiz sayfa indeksi: {pageIndex}");
            return;
        }

        // Eski sayfayı kaldır
        if (currentPageInstance != null)
        {
            Destroy(currentPageInstance);
        }

        currentPageIndex = pageIndex;
        ComicPage pagePrefab = pagesPrefabs[pageIndex];

        // Prefab'ı spawn et
        if (canvasTransform == null)
        {
            canvasTransform = FindObjectOfType<Canvas>()?.transform;
        }

        if (canvasTransform != null)
        {
            currentPageInstance = Instantiate(pagePrefab.gameObject, canvasTransform);
            currentPage = currentPageInstance.GetComponent<ComicPage>();
        }
        else
        {
            Debug.LogError("[ComicManager] Canvas bulunamadı!");
            return;
        }

        // Animator Controller'ı değiştir
        if (cameraAnimator != null && currentPage != null)
        {
            RuntimeAnimatorController controller = currentPage.GetAnimatorController();
            cameraAnimator.runtimeAnimatorController = controller;
            Debug.Log($"[ComicManager] Sayfa {pageIndex} yüklendi: {currentPage.name}");
        }
    }

    /// <summary>
    /// Sonraki panele geç
    /// Progress to next panel
    /// </summary>
    public void NextPanel()
    {
        if (currentPage == null)
        {
            Debug.LogWarning("[ComicManager] Hiç sayfa yüklenmemiş!");
            return;
        }

        // Eğer animasyon bittikten sonra tıklama bekliyorsa, bekleyen aksiyon gerçekleş
        if (isWaitingForNextInput)
        {
            isWaitingForNextInput = false;
            ExecuteNextAction(pendingNextAction);
            return;
        }

        int panelCount = currentPage.GetPanelCount();
        int nextPanelIndex = currentPage.GetCurrentPanelIndex() + 1;

        // Sayfa panelleri bitti mi?
        if (nextPanelIndex >= panelCount)
        {
            OnPageEnded();
            return;
        }

        // Animator'ü tetikle
        currentPage.AdvancePanel();

        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("next");
        }

        Debug.Log($"[ComicManager] Panel ilerletildi: {nextPanelIndex}/{panelCount}");
    }

    /// <summary>
    /// Sayfa sona erdi - animasyon oynat ve tıklama bekle
    /// Page ended - play animation and wait for next click
    /// </summary>
    private void OnPageEnded()
    {
        Debug.Log($"[ComicManager] Sayfa {currentPageIndex} sona erdi");

        string nextAction = currentPage.GetNextAction();

        // Son panelin animasyonunu oynat
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("next");
        }

        // Animasyonun bitmesini bekle, sonra tıklama bekleme moduna geç
        StartCoroutine(WaitForAnimationAndPrepareForNextInput(nextAction));
    }

    /// <summary>
    /// Animasyonun bitmesini bekle, sonra tıklama bekleme moduna geç
    /// Wait for animation to finish, then wait for next input
    /// </summary>
    private IEnumerator WaitForAnimationAndPrepareForNextInput(string nextAction)
    {
        if (cameraAnimator != null)
        {
            // Animatör'ün animation clip'ini al
            AnimatorStateInfo stateInfo = cameraAnimator.GetCurrentAnimatorStateInfo(0);

            // Mevcut animation'ın bitmesini bekle
            yield return new WaitForSeconds(stateInfo.length);

            Debug.Log($"[ComicManager] Animasyon tamamlandı ({stateInfo.length}s), tıklama bekleniyor...");
        }

        // Tıklama bekleme moduna geç
        pendingNextAction = nextAction;
        isWaitingForNextInput = true;
    }

    /// <summary>
    /// Bekleyen aksionu gerçekleştir
    /// Execute pending action
    /// </summary>
    private void ExecuteNextAction(string nextAction)
    {
        // Sonraki sayfa var mı?
        if (currentPageIndex + 1 < pagesPrefabs.Length && nextAction == "nextPage")
        {
            Debug.Log($"[ComicManager] Sonraki Comic'e geçiliyor...");
            LoadPage(currentPageIndex + 1);
        }
        else if (nextAction == "fightscene")
        {
            // Fight Scene'e geç
            Debug.Log($"[ComicManager] Fight Scene yükleniyor: fightscene");
            UnityEngine.SceneManagement.SceneManager.LoadScene("fightscene");
        }
        else if (!string.IsNullOrEmpty(nextAction) && nextAction != "end" && nextAction != "nextPage")
        {
            // Özel Scene yükle
            Debug.Log($"[ComicManager] Scene yükleniyor: {nextAction}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextAction);
        }
        else
        {
            Debug.Log("[ComicManager] Comic sona erdi!");
        }
    }

    public ComicPage GetCurrentPage() => currentPage;
    public int GetCurrentPageIndex() => currentPageIndex;
}