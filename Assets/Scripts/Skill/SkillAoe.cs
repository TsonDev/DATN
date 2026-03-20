using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAoe : MonoBehaviour
{
    [SerializeField] GameObject auraObject;
    [SerializeField] float auraDuration = 3f;
    [SerializeField] GameObject player;

    float timer;
    bool auraActive;

    void Update()
    {
        if (!auraActive) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            auraObject.SetActive(false);
            auraActive = false;
        }
    }

    public void ActivateAura()
    {
        auraObject.transform.position = player.transform.position;
        auraObject.SetActive(true);
        timer = auraDuration;
        auraActive = true;
    }
}
