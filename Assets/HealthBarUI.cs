using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;

    public void UpdateHealth(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
