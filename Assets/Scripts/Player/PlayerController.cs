using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using static DameType;


public class PlayerController : MonoBehaviour
{
    
    [SerializeField] int maxHealth = 100;
    int currentHealth;
    public float timeInvicible = 1f;
    float timedameCoolDown;
    bool isNatureInvicible;
    bool isInvicible;

    [SerializeField] float fireCooldown = 0.12f;
    float nextFireTime;
    public GameObject ProjectTilePrefab;
    SkillAoe skillAoe;
    Animator animator;
    PlayerMovement playerMovement;
    Rigidbody2D rig2d;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rig2d = GetComponent<Rigidbody2D>();
        skillAoe = GetComponent<SkillAoe>();
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()   
    {
        if (isInvicible)
        {
            timedameCoolDown -= Time.deltaTime;
            if (timedameCoolDown < 0)
            {
                isInvicible=false;
            }
        }
    }
    //public void Attack1(InputAction.CallbackContext context)
    //{
    //    // Nếu đang ở state Attack1 thì không cho trigger lại
    //    if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
    //        return;
    //    if (context.performed)
    //    {
    //        GameObject gameObject = GameObject.Instantiate(ProjectTilePrefab,rig2d.position,Quaternion.identity);
    //        ProjectTile projectTile = gameObject.GetComponent<ProjectTile>();
    //        Vector2 dirLast = playerMovement.lastDir;
    //        Debug.Log(dirLast);
    //        projectTile.LunchProTile(dirLast, 300);
    //        animator.SetTrigger("Attack1");
    //    }

    //}
    public void ChangeHealth(int amount,DameType.TypeDamage type)
    {
        if (amount < 0)
        {
            if (isInvicible)
            {
                return;
            }
            if(type == DameType.TypeDamage.Snow && isNatureInvicible)
            {
                return;
            }
            
                isInvicible = true;
                timedameCoolDown = timeInvicible;
            
            
        }
        currentHealth = Mathf.Clamp(currentHealth+amount, 0, maxHealth);
        Debug.Log("currentHealth"+currentHealth);
    }
    public void ActivateNatureResistant(float time)
    {
        StartCoroutine(NatureResistantCoroutine(time));
    }

    IEnumerator NatureResistantCoroutine(float time)
    {
        isNatureInvicible = true;

        yield return new WaitForSeconds(time);

        isNatureInvicible = false;
    }
    public void Attack1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireCooldown;

        Vector2 dir = (playerMovement.moveInput.sqrMagnitude > 0.001f)
            ? playerMovement.moveInput.normalized
            : playerMovement.lastDir; // nhớ lastDir đã normalized

        var go = Instantiate(ProjectTilePrefab, rig2d.position + dir*0.5f, Quaternion.identity);
        go.GetComponent<ProjectTile>().LunchProTile(dir, 300);

        // khóa hướng cho animation (nếu animation cần)
        animator.SetFloat("lastInputX", dir.x);
        animator.SetFloat("lastInputY", dir.y);
        animator.SetTrigger("Attack1");
    }
    public void Skill1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        skillAoe.ActivateAura();

        animator.SetTrigger("Skill1");
    }
    public void MovePlayer(InputAction.CallbackContext context)
    {
        playerMovement.Move(context);
    }
}
