using UnityEngine;
using System.Collections;

/// <summary>
/// Kamera hareketi Animator tarafından kontrol edilir
/// Camera movement is controlled by Animator
/// 
/// Bu script kamera pozisyonunun başlangıç ayarı ve event callback'ler için
/// This script is for initial camera position setup and event callbacks
/// </summary>
public class ComicCameraController : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("[ComicCameraController] Camera component bulunamadı!");
        }
        else
        {
            Debug.Log("[ComicCameraController] Kamera hazırlandı (Animator tarafından kontrol edilecek)");
        }
    }

    /// <summary>
    /// OPSIYONEL: Ortho size değiştir (Animator'ün state'inde de yapılabilir)
    /// OPTIONAL: Change ortho size (can also be done in Animator states)
    /// </summary>
    public void SetOrthographicSize(float size)
    {
        if (cam != null)
        {
            cam.orthographicSize = size;
        }
    }

    /// <summary>
    /// Kamerayı konuma taşı (smooth kordu değildir - Animator animasyonu kullanın)
    /// Move camera to position (not smooth - use Animator animation)
    /// </summary>
    public void SetCameraPosition(Vector3 position)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }
}
