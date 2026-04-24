using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarUI : MonoBehaviour
{
    public Image fillImage;

    public void UpdateMana(float current, float max)
    {
        fillImage.fillAmount = current / max;
    }
}
