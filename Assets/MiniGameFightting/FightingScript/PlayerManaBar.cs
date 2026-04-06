using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ManaBar : MonoBehaviour
{
	public Slider manaSlider;
	private ManaSystem playerMana;

	private void Start()
	{
		StartCoroutine(WaitForPlayer());
	}

	private IEnumerator WaitForPlayer()
	{
		GameObject player = null;
		while (player == null)
		{
			yield return new WaitForSeconds(0.1f);
			player = GameObject.FindGameObjectWithTag("Player");
			yield return null;
		}

		playerMana = player.GetComponent<ManaSystem>();
		playerMana.manaChanged.AddListener(OnManaChanged);
		UpdateManaBar(playerMana.CurrentMana, playerMana.MaxMana);
	}

	private void OnDisable()
	{
		if (playerMana != null)
			playerMana.manaChanged.RemoveListener(OnManaChanged);
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
