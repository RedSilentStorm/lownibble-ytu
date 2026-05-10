using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class QTEDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private Button clickButton;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text typeLabel; // "Dodge", "Parry" vs.

    private QTEType type;
    private bool isCompleted = false;
    private QTEComplexity complexity;
    private KeyCode requiredKey;
    private List<KeyCode> comboSequence;
    private int comboIndex = 0;
    private ReactionUI parentUI;

    public void Setup(QTEData data, KeyCode randomKey, ReactionUI ui, KeyCode[] keyPool)
    {
        type = data.type;
        isCompleted = false;
        complexity = data.complexity;
        parentUI = ui;
        requiredKey = randomKey;

        typeLabel.text = type.ToString();

        if (complexity == QTEComplexity.Simple)
        {
            keyText.text = requiredKey.ToString().Replace("Alpha", "");
            clickButton.gameObject.SetActive(false);
            comboText.gameObject.SetActive(false);
        }
        else if (complexity == QTEComplexity.Medium)
        {
            keyText.gameObject.SetActive(false);
            clickButton.gameObject.SetActive(true);
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => { parentUI.NotifySuccess(type); });
        }
        else if (complexity == QTEComplexity.Combo)
        {
            int length = Random.Range(2, 4);
            comboSequence = new List<KeyCode>();
            for (int i = 0; i < length; i++)
            {
                comboSequence.Add(keyPool[Random.Range(0, keyPool.Length)]);
            }
            comboIndex = 0;
            // Combo'yu "W-A-D" gibi göster
            string comboString = string.Join("-", comboSequence.ConvertAll(k => k.ToString().Replace("Alpha", "")));
            keyText.text = comboString;
            comboText.text = $"1/{length}";
            clickButton.gameObject.SetActive(false);
        }
    }

    public void OnKeyPressed(KeyCode key)
    {
        Debug.Log($"QTEDisplay({type}): Basılan tuş = {key}, Beklenen = {(comboSequence != null && comboIndex < comboSequence.Count ? comboSequence[comboIndex].ToString() : requiredKey.ToString())}");
        if (!ReactionWindowManager.Instance.IsWindowOpen) return;
        if (isCompleted) return;

        // Sadece Simple veya Combo ise tuş kontrolü yap, Medium ise butonla çalışır
        if (complexity == QTEComplexity.Medium) return;

        if (complexity == QTEComplexity.Simple)
        {
            if (key == requiredKey)
            {
                parentUI.NotifySuccess(type);
                isCompleted = true;
            }
            // Yanlış tuş → hiçbir şey yapma
        }
        else if (complexity == QTEComplexity.Combo)
        {
            if (key == comboSequence[comboIndex])
            {
                comboIndex++;
                if (comboIndex >= comboSequence.Count)
                {
                    parentUI.NotifySuccess(type);
                    isCompleted = true;
                }
                else
                {
                    comboText.text = $"{comboIndex + 1}/{comboSequence.Count}";
                }
            }
            else
            {
                // Yanlış tuş, kombo başarısız
                ReactionWindowManager.Instance.ForceFail();
            }
        }
    }
}