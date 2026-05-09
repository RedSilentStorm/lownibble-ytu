using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class FightUI : MonoBehaviour
{
    public static FightUI Instance { get; private set; }

    [SerializeField] private GameObject actionPanel;
    [SerializeField] private GameObject abilityButtonPrefab;  // Yeni
    [SerializeField] private Transform buttonContainer;      // Butonların yerleşeceği panel

    private Action<AbilityData> onAbilitySelected;
    private bool waitingForCommand = false;
    private List<GameObject> spawnedButtons = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
        actionPanel.SetActive(false);
    }

    public void ShowActions(BattleEntity currentEntity, Action<AbilityData> callback)
    {
        if (waitingForCommand) return;
        waitingForCommand = true;
        onAbilitySelected = callback;

        // Eski butonları temizle
        foreach (var btn in spawnedButtons)
            Destroy(btn);
        spawnedButtons.Clear();

        // Karakterin yetenekleri kadar buton oluştur
        foreach (var ability in currentEntity.GetAbilities())
        {
            GameObject newButton = Instantiate(abilityButtonPrefab, buttonContainer);
            TMP_Text tmpText = newButton.GetComponentInChildren<TMP_Text>();
            tmpText.text = ability.abilityName;
            newButton.GetComponent<Button>().onClick.AddListener(() => OnAbilityClicked(ability));
            spawnedButtons.Add(newButton);
        }

        actionPanel.SetActive(true);
    }

    private void OnAbilityClicked(AbilityData ability)
    {
        if (!waitingForCommand) return;
        waitingForCommand = false;
        actionPanel.SetActive(false);
        onAbilitySelected?.Invoke(ability);
    }
}