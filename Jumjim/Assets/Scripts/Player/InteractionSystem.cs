using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface IInteractable
{
    public void Interact();
}
public class InteractionSystem : MonoBehaviour
{
    public Transform InteractorSource;
    public float InteractRange = 3f;
    public LayerMask InteractableLayer;
    public LayerMask ObstacleLayer;
    public GameObject interactUI;

    private void Update()
    {
        Ray r = new Ray(InteractorSource.position, InteractorSource.forward);

        if (Physics.Raycast(r, out RaycastHit hit, InteractRange, InteractableLayer))
        {
            // Check if there’s an obstacle between the player and the interactable
            if (!Physics.Raycast(r, hit.distance, ObstacleLayer))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactObj))
                {
                    if (interactUI != null)
                        interactUI.SetActive(true);

                    if (Input.GetKeyDown(KeyCode.E))
                        interactObj.Interact();

                    return; // Exit early if successful
                }
            }
        }

        if (interactUI != null)
            interactUI.SetActive(false);
    }
}

