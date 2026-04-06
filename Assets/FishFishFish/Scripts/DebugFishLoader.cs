using UnityEngine;
using System.Collections.Generic;

public class DebugFishLoader : MonoBehaviour
{
	void Start()
	{
		Debug.Log("===== DEBUG BUILD START =====");

		// Kiểm tra có sprite nào trong thư mục Resources/Fish/
		Sprite[] sprites = Resources.LoadAll<Sprite>("Fish");
		Debug.Log($"[DEBUG] Resources.LoadAll<Sprite>(\"Fish\") = {sprites.Length}");
		foreach (var s in sprites)
		{
			Debug.Log($"[DEBUG] Found sprite: {s.name}");
		}

		// Kiểm tra Grid object
		var grid = FindObjectOfType<Grid>();
		if (grid == null)
		{
			Debug.LogError("[DEBUG] ❌ Không tìm thấy Grid trong scene!");
		}
		else
		{
			Debug.Log("[DEBUG] ✅ Grid tồn tại trong scene.");
		}

		// Kiểm tra prefab cá
		var colorPieces = FindObjectsOfType<ColorPiece>();
		Debug.Log($"[DEBUG] ColorPiece instances in scene: {colorPieces.Length}");
		foreach (var c in colorPieces)
		{
			var renderer = c.GetComponentInChildren<SpriteRenderer>();
			if (renderer == null)
			{
				Debug.LogError($"[DEBUG] ❌ {c.name}: Không tìm thấy SpriteRenderer.");
			}
			else
			{
				Debug.Log($"[DEBUG] ✅ {c.name}: SpriteRenderer.sprite = {(renderer.sprite != null ? renderer.sprite.name : "NULL")}");
			}
		}

		Debug.Log("===== DEBUG BUILD END =====");
	}
}
