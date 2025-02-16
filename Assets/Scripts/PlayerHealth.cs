using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth, healAmount;
    public static float health = 100;
    public bool invincible;
    public bool dead;

    public float healthPercent { get { return health / maxHealth; } }

    [SerializeField] Slider healthBar;
    [SerializeField] GameObject ui;

    private void Awake()
    {
        healthBar.value = healthPercent;
    }

    public void TakeDamage(float damage)
    {
        if (dead) return;

        if (!invincible)
        {
            health -= damage;
            UpdateHealthBar(false);
        }

        if (health <= 0)
        {
            OnDeath();
        }
    }

    public void AbsorbHealth()
    {
        if (healAmount > 0)
        {
            health += maxHealth * healAmount;
            if (health > maxHealth) health = maxHealth;
            UpdateHealthBar(true);
        }
    }

    public void HealByPercent(float healPercent)
    {
        HealByAmount(maxHealth * healPercent * 0.01f);
    }

    public void HealByAmount(float healAmount)
    {
        if (health >= maxHealth)
        {
            return;
        }

        health += healAmount;
        if (health > maxHealth) health = maxHealth;
        UpdateHealthBar(true);
    }

    private void UpdateHealthBar(bool healthGained)
    {
        if (healthGained)
        {
            healthBar.value = healthPercent;
        }
        else
        {
            healthBar.value = healthPercent;
        }
    }

    private void OnDeath()
    {
        dead = true;
    }
}
