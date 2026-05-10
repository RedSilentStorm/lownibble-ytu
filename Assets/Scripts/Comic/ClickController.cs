using UnityEngine;

/// <summary>
/// Input handler - Fare/Touch kliklerini dinler
/// Input handler - Listens to mouse/touch clicks
/// 
/// ComicManager'a NextPanel() çağrısı yaparak modüler kalır
/// Stays modular by calling ComicManager.NextPanel()
/// </summary>
public class ClickController : MonoBehaviour
{
    void Update()
    {
        // Sol fare tuşu basıldı mı?
        if (Input.GetMouseButtonDown(0))
        {
            ComicManager.Instance.NextPanel();
        }

        // OPSIYONEL: Space tuşu ile de kontrol et
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ComicManager.Instance.NextPanel();
        }
    }
}