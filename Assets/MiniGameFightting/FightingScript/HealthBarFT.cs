using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class HealthBarFT : MonoBehaviour
{
	public Slider healthSlider;
	private Damageable playerDamageable;
	private Coroutine waitCoroutine;

	private void OnEnable()
	{
		// Start tìm player khi object UI bật
		waitCoroutine = StartCoroutine(WaitForPlayer());
	}

	private void OnDisable()
	{
		// Stop coroutine chờ nếu UI bị disable
		if (waitCoroutine != null)
		{
			StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}

		// Remove listener nếu mọi thứ hợp lệ
		if (playerDamageable != null)
		{
			// Kiểm tra chắc chắn event không null trước khi remove
			var evt = playerDamageable.healthChanged;
			if (evt != null)
			{
				evt.RemoveListener(OnPlayerHealthChanged);
			}
		}
	}

	private IEnumerator WaitForPlayer()
	{
		GameObject player = null;

		// đợi player có tag "Player"
		while (player == null)
		{
			yield return new WaitForSeconds(0.1f);
			player = GameObject.FindGameObjectWithTag("Player");
			yield return null;
		}

		// Lấy component Damageable, có kiểm tra an toàn
		if (!player.TryGetComponent<Damageable>(out playerDamageable) || playerDamageable == null)
		{
			Debug.LogError("Player vừa spawn nhưng không có Damageable!");
			yield break;
		}

		// Nếu event tồn tại thì add listener
		if (playerDamageable.healthChanged != null)
		{
			playerDamageable.healthChanged.AddListener(OnPlayerHealthChanged);
		}
		else
		{
			Debug.LogWarning("playerDamageable.healthChanged is null");
		}

		// cập nhật thanh máu lần đầu
		UpdateHealthBar(playerDamageable.Health, playerDamageable.MaxHealth);
		waitCoroutine = null;
	}

	private void OnPlayerHealthChanged(int newHealth, int maxHealth)
	{
		UpdateHealthBar(newHealth, maxHealth);
	}

	private void UpdateHealthBar(float currentHealth, float maxHealth)
	{
		if (healthSlider == null) return;
		if (maxHealth <= 0f) healthSlider.value = 0f;
		else healthSlider.value = currentHealth / maxHealth;
	}
}
