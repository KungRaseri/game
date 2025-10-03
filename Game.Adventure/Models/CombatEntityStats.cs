using System;

namespace Game.Adventure.Models;

/// <summary>
/// Generic combat entity stats that can represent any fighting unit (adventurer, monster, etc.)
/// </summary>
public class CombatEntityStats
{
    private int _currentHealth;
    private int _maxHealth;
    private int _damagePerSecond;
    private string _name;
    private float _retreatThreshold;

    public int CurrentHealth 
    { 
        get => _currentHealth;
        private set 
        {
            _currentHealth = Math.Max(0, Math.Min(value, _maxHealth));
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }

    public int MaxHealth 
    { 
        get => _maxHealth;
        set 
        {
            _maxHealth = Math.Max(1, value);
            CurrentHealth = Math.Min(_currentHealth, _maxHealth);
        }
    }

    public int DamagePerSecond 
    { 
        get => _damagePerSecond;
        set => _damagePerSecond = Math.Max(1, value);
    }

    public string Name 
    { 
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public float RetreatThreshold 
    { 
        get => _retreatThreshold;
        set => _retreatThreshold = Math.Clamp(value, 0f, 1f);
    }

    public float HealthPercentage => (float)CurrentHealth / MaxHealth;
    public bool ShouldRetreat => HealthPercentage < RetreatThreshold;
    public bool IsAlive => CurrentHealth > 0;

    public event Action<int, int>? HealthChanged;
    public event Action<CombatEntityStats>? Died;

    public CombatEntityStats(string name, int maxHealth, int damagePerSecond, float retreatThreshold = 0f)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _maxHealth = Math.Max(1, maxHealth);
        _damagePerSecond = Math.Max(1, damagePerSecond);
        _retreatThreshold = Math.Clamp(retreatThreshold, 0f, 1f);
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || !IsAlive) return;
        
        CurrentHealth -= damage;
        
        if (!IsAlive)
        {
            Died?.Invoke(this);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHealth += amount;
    }

    public void FullHeal()
    {
        CurrentHealth = MaxHealth;
    }

    public void RegenerateHealth(int amount = 1)
    {
        if (CurrentHealth < MaxHealth)
        {
            Heal(amount);
        }
    }

    public override string ToString()
    {
        return $"{Name} (HP: {CurrentHealth}/{MaxHealth}, DPS: {DamagePerSecond})";
    }
}
