using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDerector : MonoBehaviour
{
    private IInteractable interactableInRange = null; //Closest Interactable
    public GameObject interactionIcon;
    private void Start()
    {
        interactionIcon.SetActive(false);
    }
    public void OnInteract(InputAction.CallbackContext context)
    {
        //if(context.performed)
        //{
        //    interactableInRange?.Interact();
        //    if (!interactableInRange.CanInteract())
        //    {
        //        interactionIcon.SetActive(false);
        //    }
        //}
        if(!context.performed || interactableInRange == null) return;

        interactableInRange.Interact();

        if (!interactableInRange.CanInteract())
            interactionIcon.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
