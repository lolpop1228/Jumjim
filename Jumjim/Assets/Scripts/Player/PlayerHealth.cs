using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public TextMeshProUGUI healthText;

    [Header("Hurt Screen")]
    public Image hurtScreen;
    public float hurtFadeDuration = 0.5f;
    public CameraShake cameraShake;
    private Color originalColor;

    void Start()
    {
        GameObject healthUI = GameObject.FindWithTag("HealthUI");
        if (healthUI != null)
        {
            healthText = healthUI.GetComponent<TextMeshProUGUI>();
        }

        if (hurtScreen != null)
        {
            originalColor = new Color(1f, 0f, 0f, 0.1f);;
            SetAlpha(0f);
        }

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Health Left:" + currentHealth);
        UpdateHealthUI();
        ShowHurtScreen();

        if (cameraShake != null)
        {
            cameraShake.Shake();
        }

        if (currentHealth <= 0)
            {
                Die();
            }
    }

    void Die()
    {
        Debug.Log("Player died.");
    }

    public void HealPlayer(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthText == null) return;

        healthText.text = $"Health: {currentHealth}";
    }

    void ShowHurtScreen()
    {
        if (hurtScreen != null)
            StartCoroutine(FadeHurtScreen());
    }

    System.Collections.IEnumerator FadeHurtScreen()
    {
        SetAlpha(1f); // Full red flash

        float elapsed = 0f;
        while (elapsed < hurtFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / hurtFadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f); // Ensure it's invisible at the end
    }

    void SetAlpha(float alpha)
    {
        if (hurtScreen != null)
        {
            Color c = originalColor;
            c.a = alpha;
            hurtScreen.color = c;
        }
    }
}
