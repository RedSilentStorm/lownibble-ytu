using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;
    public TMP_Text healthText;

    private int currentHealth;
    private int maxHealth;

    public void Setup(int maxHp)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        UpdateUI();
    }

    public void UpdateHealth(int newHealth)
    {
        currentHealth = newHealth;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    void UpdateUI()
    {
        float ratio = (float)currentHealth / maxHealth;
        fillImage.fillAmount = ratio;

        healthText.text = currentHealth + " / " + maxHealth;
    }
}
