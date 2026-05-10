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
        Debug.Log($"ShowActions çağrıldı. Yetenek sayısı: {currentEntity.GetAbilities().Count}, waitingForCommand: {waitingForCommand}");
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
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            Button button = newButton.GetComponent<Button>();

            bool onCooldown = currentEntity.IsOnCooldown(ability);
            int remaining = currentEntity.GetCooldownRemaining(ability);

            if (onCooldown)
            {
                buttonText.text = $"{ability.abilityName} ({remaining})";
                button.interactable = false; // tıklanamaz
            }
            else
            {
                buttonText.text = ability.abilityName;
                button.interactable = true;
                button.onClick.AddListener(() => OnAbilityClicked(ability));
            }

            // Görsel olarak gri yapmak için isteğe bağlı
            button.image.color = onCooldown ? Color.gray : Color.white;

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

    public void ShowTargetSelection(List<BattleEntity> targets, Action<BattleEntity> callback)
    {
        if (waitingForCommand) return;
        waitingForCommand = true;

        // Eski butonları temizle
        foreach (var btn in spawnedButtons) Destroy(btn);
        spawnedButtons.Clear();

        foreach (var target in targets)
        {
            GameObject newButton = Instantiate(abilityButtonPrefab, buttonContainer);
            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            Button button = newButton.GetComponent<Button>();
            buttonText.text = target.data.characterName;
            button.onClick.AddListener(() =>
            {
                waitingForCommand = false;
                callback(target);
                // Panel kapatma (yetenek seçimi açılacak)
                actionPanel.SetActive(false);
            });
            spawnedButtons.Add(newButton);
        }

        actionPanel.SetActive(true);
    }
}