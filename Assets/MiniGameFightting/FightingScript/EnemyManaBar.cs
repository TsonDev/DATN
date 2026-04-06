using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyManaBar : MonoBehaviour
{
	public Slider manaSlider;
	private ManaSystem enemyMana;

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

		enemyMana = enemy.GetComponent<ManaSystem>();
		enemyMana.manaChanged.AddListener(OnManaChanged);
		UpdateManaBar(enemyMana.CurrentMana, enemyMana.MaxMana);
	}

	private void OnDisable()
	{
		if (enemyMana != null)
			enemyMana.manaChanged.RemoveListener(OnManaChanged);
	}

	private void OnManaChanged(int newMana, int maxMana)
	{
		UpdateManaBar(newMana, maxMana);
	}

	private void UpdateManaBar(float currentMana, float maxMana)
	{
		manaSlider.value = currentMana / maxMana;
	}
}
