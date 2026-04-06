using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ManaSystem : MonoBehaviour
{
	[Header("Mana Settings")]
	[SerializeField] private int _maxMana = 100;
	[SerializeField] private int _currentMana = 100;
	[SerializeField] private float manaRegenRate = 5f; // hồi mana mỗi giây

	public UnityEvent<int, int> manaChanged; // (currentMana, maxMana)

	private bool isRegenerating = true;

	public int MaxMana
	{
		get => _maxMana;
		set
		{
			_maxMana = Mathf.Max(1, value);
			manaChanged?.Invoke(_currentMana, _maxMana);
		}
	}

	public int CurrentMana
	{
		get => _currentMana;
		set
		{
			_currentMana = Mathf.Clamp(value, 0, MaxMana);
			manaChanged?.Invoke(_currentMana, MaxMana);
		}
	}

	private void Start()
	{
		CurrentMana = MaxMana;
		if (isRegenerating)
			StartCoroutine(RegenMana());
	}

	private IEnumerator RegenMana()
	{
		while (isRegenerating)
		{
			yield return new WaitForSeconds(1f);
			AddMana((int)manaRegenRate);
		}
	}

	public bool UseMana(int amount)
	{
		if (CurrentMana >= amount)
		{
			CurrentMana -= amount;
			return true;
		}

		// không đủ mana thì không cast skill
		Debug.Log("❌ Not enough mana!");
		return false;
	}

	public void AddMana(int amount)
	{
		if (amount > 0)
		{
			CurrentMana = Mathf.Min(CurrentMana + amount, MaxMana);
		}
	}

	public void GainManaFromDamage(int damage)
	{
		int manaGain = Mathf.RoundToInt(damage * 0.5f);
		AddMana(manaGain);
	}

	public void StopRegen()
	{
		isRegenerating = false;
		StopAllCoroutines();
	}

	public void ResumeRegen()
	{
		if (!isRegenerating)
		{
			isRegenerating = true;
			StartCoroutine(RegenMana());
		}
	}
}
