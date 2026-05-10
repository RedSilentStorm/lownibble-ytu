using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor Script - Sayfaları otomatik prefab'a dönüştürmek için
/// Editor Script - Auto-convert pages to prefabs
/// 
/// KULLANIM:
/// 1. Canvas'ı seç
/// 2. Assets → Comic → Create Page Prefabs
/// </summary>
#if UNITY_EDITOR
public class ComicPagePrefabCreator
{
    [MenuItem("Assets/Comic/Create Page Prefabs from Hierarchy")]
    public static void CreatePagePrefabs()
    {
        Canvas canvas = Selection.activeGameObject?.GetComponent<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Hata", "Canvas seçin!", "OK");
            return;
        }

        // Prefabs folder oluştur
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        int pageCount = 0;
        foreach (Transform child in canvas.transform)
        {
            if (child.name.StartsWith("Page_") || child.name.StartsWith("Page"))
            {
                string prefabPath = $"Assets/Prefabs/{child.name}.prefab";
                
                // ComicPage component'i varsa devam et
                ComicPage page = child.GetComponent<ComicPage>();
                if (page == null)
                {
                    page = child.gameObject.AddComponent<ComicPage>();
                }

                // Prefab oluştur
                PrefabUtility.SaveAsPrefabAsset(child.gameObject, prefabPath);
                pageCount++;
                
                Debug.Log($"[ComicPagePrefabCreator] Prefab oluşturuldu: {prefabPath}");
            }
        }

        EditorUtility.DisplayDialog("Başarılı!", $"{pageCount} prefab oluşturuldu!", "OK");
        AssetDatabase.Refresh();
    }
}
#endif
