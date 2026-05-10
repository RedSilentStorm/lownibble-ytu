using UnityEngine;

/// <summary>
/// Her Comic Page için component - panelleri ve animatör controller'ı tutacak
/// Component for each Comic Page - holds panels and animator controller
/// 
/// Bu script prefab'ın bir parçası olur ve sayfa-spesifik ayarları tutar
/// This script is part of the prefab and holds page-specific settings
/// </summary>
public class ComicPage : MonoBehaviour
{
    [Header("Sayfa Ayarları / Page Settings")]
    [SerializeField] private string pageName = "Page 1";
    [SerializeField] private int panelCount = 6;

    [Header("Animator Controller / Animator Controller")]
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Sonraki Aksiyon / Next Action")]
    [SerializeField] private PageEndAction endAction = PageEndAction.NextComic;
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private int nextPageIndex = -1;

    private int currentPanelIndex = 0;

    void Start()
    {
        // Sayfa başladığında 0. panelden başla
        currentPanelIndex = 0;
        Debug.Log($"[ComicPage] Sayfa başladı: {pageName}");
    }

    /// <summary>
    /// Sonraki panele ilerlet
    /// Advance to next panel
    /// </summary>
    public void AdvancePanel()
    {
        if (currentPanelIndex < panelCount - 1)
        {
            currentPanelIndex++;
        }
    }

    /// <summary>
    /// Animator Controller'ı döndür
    /// Return the Animator Controller for this page
    /// </summary>
    public RuntimeAnimatorController GetAnimatorController()
    {
        return animatorController;
    }

    /// <summary>
    /// Panel sayısı döndür
    /// Return panel count
    /// </summary>
    public int GetPanelCount()
    {
        return panelCount;
    }

    /// <summary>
    /// Mevcut panel indeksi döndür
    /// Return current panel index
    /// </summary>
    public int GetCurrentPanelIndex()
    {
        return currentPanelIndex;
    }

    /// <summary>
    /// Sayfa bittiğinde ne yapılacağını döndür
    /// Return what action to take when page ends
    /// </summary>
    public string GetNextAction()
    {
        switch (endAction)
        {
            case PageEndAction.NextComic:
                return "nextPage";
            case PageEndAction.FightScene:
                return "Fight";
            case PageEndAction.CustomScene:
                return nextSceneName;
            case PageEndAction.End:
                return "end";
            default:
                return "end";
        }
    }

    /// <summary>
    /// Debug: Bilgi yazdır
    /// Debug: Print info
    /// </summary>
    public void PrintInfo()
    {
        Debug.Log($"[ComicPage] {pageName} - Panel: {currentPanelIndex + 1}/{panelCount}");
    }
}

/// <summary>
/// Sayfa bittiğinde yapılacak aksiyonlar
/// Actions to take when page ends
/// </summary>
public enum PageEndAction
{
    NextComic,   // Sonraki Comic'e geç
    FightScene,  // Fight Scene'e geç
    CustomScene, // Özel Scene yükle
    End          // Bitir
}
