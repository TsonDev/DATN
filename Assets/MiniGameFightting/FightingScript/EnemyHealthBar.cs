using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthBar : MonoBehaviour
{
	public Slider healthSlider;
	private Damageable enemyDamageable;

	private void Start()
	{
		StartCoroutine(WaitForEnemy());
	}

	private IEnumerator WaitForEnemy()
	{
		GameObject enemy = null;
		while (enemy == null)
		{
			yield return new WaitForSeconds(0.1f);
			enemy = GameObject.FindGameObjectWithTag("Enemy");
			yield return null;
		}

		enemyDamageable = enemy.GetComponent<Damageable>();
		enemyDamageable.healthChanged.AddListener(OnEnemyHealthChanged);
		UpdateHealthBar(enemyDamageable.Health, enemyDamageable.MaxHealth);
	}

	private void OnDisable()
	{
		if (enemyDamageable != null)
			enemyDamageable.healthChanged.RemoveListener(OnEnemyHealthChanged);
	}

	private void OnEnemyHealthChanged(int newHealth, int maxHealth)
	{
		UpdateHealthBar(newHealth, maxHealth);
	}

	private void UpdateHealthBar(float currentHealth, float maxHealth)
	{
		healthSlider.value = currentHealth / maxHealth;
	}
}
