using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillData
{
    public GameObject prefab;           // Prefab của skill
    public Transform spawnPoint;        // Điểm spawn skill
    public float cooldown = 1f;         // Thời gian hồi chiêu
    [HideInInspector] public bool isOnCooldown = false; // trạng thái cooldown
}
public class SkillManager : MonoBehaviour
{
	[Header("Skills Settings")]
	public List<SkillData> skills = new List<SkillData>();

	// Animation Event gọi trực tiếp mấy hàm này
	public void CastSkill0()
	{
		CastSkill(0, transform.localScale);
	}

	public void CastSkill1()
	{
		CastSkill(1, transform.localScale);
	}

	public void CastSkill2()
	{
		CastSkill(2, transform.localScale);
	}

	// Hàm gốc
	public void CastSkill(int index, Vector3 playerScale)
	{
		if (index < 0 || index >= skills.Count) return;

		SkillData skill = skills[index];

		if (!skill.isOnCooldown)
		{
			GameObject skillPush = Instantiate(skill.prefab,
											   skill.spawnPoint.position,
											   skill.prefab.transform.rotation);

			Vector3 origScale = skillPush.transform.localScale;
			skillPush.transform.localScale = new Vector3(
				origScale.x * (playerScale.x > 0 ? 1 : -1),
				origScale.y,
				origScale.z
			);

			StartCoroutine(SkillCooldown(skill));
		}
		else
		{
			Debug.Log($"Skill {index} đang hồi chiêu...");
		}
	}

	private IEnumerator SkillCooldown(SkillData skill)
	{
		skill.isOnCooldown = true;
		yield return new WaitForSeconds(skill.cooldown);
		skill.isOnCooldown = false;
	}
}
