using UnityEngine;

public class IndicatorFollowTarget : MonoBehaviour
{
	public Transform target; // Nhân vật cần theo dõi
	public Vector3 offset = new Vector3(0, 2f, 0); // vị trí trên đầu

	void LateUpdate()
	{
		if (target != null)
		{
			transform.position = target.position + offset;
		}
	}
}
