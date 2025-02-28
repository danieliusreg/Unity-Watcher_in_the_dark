using UnityEngine;
using UnityEngine.AI;

public class MudArea : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // Sulėtinimo koeficientas

    private void OnTriggerEnter(Collider other)
    {
        // NavMeshAgent sulėtinimas
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed *= slowMultiplier;
        }

        // Veikėjo sulėtinimas
        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.speed *= slowMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // NavMeshAgent greičio grąžinimas
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed /= slowMultiplier;
        }

        // Veikėjo greičio grąžinimas
        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.speed /= slowMultiplier;
        }
    }
}

