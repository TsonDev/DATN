using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarWorld : MonoBehaviour
{
    public Image fillImage;

    // Giữ cho thanh máu không bị xoay lộn xộn nếu quái có xoay
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    public void UpdateHealth(float current, float max)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = current / max;
        }
    }
}
