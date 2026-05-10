using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ReactionUI : MonoBehaviour
{
    public static ReactionUI Instance { get; private set; }

    [SerializeField] private GameObject reactionPanel;          // ← yeni eklendi
    [SerializeField] private GameObject qteDisplayPrefab;
    [SerializeField] private Transform qteContainer;

    private List<QTEDisplay> activeDisplays = new List<QTEDisplay>();
    public List<QTEDisplay> GetActiveDisplays()
    {
        return new List<QTEDisplay>(activeDisplays); // kopyasını döndür
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ShowQTEOptions(List<QTEData> qteList, KeyCode[] dodgeKeys, KeyCode[] parryKeys, KeyCode[] counterKeys)
    {
        HideAll();
        reactionPanel.SetActive(true);      // ← paneli aç

        foreach (var qte in qteList)
        {
            GameObject obj = Instantiate(qteDisplayPrefab, qteContainer);
            QTEDisplay display = obj.GetComponent<QTEDisplay>();
            activeDisplays.Add(display);

            KeyCode requiredKey = KeyCode.None;
            KeyCode[] pool = GetKeyPool(qte.type, dodgeKeys, parryKeys, counterKeys);
            if (qte.complexity != QTEComplexity.Medium)
            {
                requiredKey = pool[Random.Range(0, pool.Length)];
            }

            display.Setup(qte, requiredKey, this, GetKeyPool(qte.type, dodgeKeys, parryKeys, counterKeys));
        }
    }

    private KeyCode[] GetKeyPool(QTEType type, KeyCode[] dodge, KeyCode[] parry, KeyCode[] counter)
    {
        switch (type)
        {
            case QTEType.Dodge: return dodge;
            case QTEType.Parry: return parry;
            case QTEType.Counter: return counter;
            default: return dodge;
        }
    }

    public void HideAll()
    {
        foreach (var d in activeDisplays)
            Destroy(d.gameObject);
        activeDisplays.Clear();

        if (reactionPanel != null)
            reactionPanel.SetActive(false);   // ← paneli kapat
    }

    // Display'den başarı bildirimi alır
    public void NotifySuccess(QTEType type)
    {
        ReactionWindowManager.Instance.ReportQTESuccess(type);
    }

}