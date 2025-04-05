using UnityEngine;
using UnityEngine.AI;

public class MudArea : MonoBehaviour
{
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            // NPC: pritaikome sulėtinimą per RandomPatrol
            RandomPatrol patrolScript = other.GetComponent<RandomPatrol>();
            if (patrolScript != null)
            {
                patrolScript.ApplySlow(slowMultiplier);
            }
        }

        // Jei žaidėjas
        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlow(slowMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            RandomPatrol patrolScript = other.GetComponent<RandomPatrol>();
            if (patrolScript != null)
            {
                patrolScript.RemoveSlow();
            }
        }

        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.RemoveSlow();
        }
    }
}
