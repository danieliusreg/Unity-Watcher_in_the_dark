using UnityEngine;
using UnityEngine.AI;

public class MudArea : MonoBehaviour
{
    public float slowMultiplier = 0.5f; // Sulėtėjimo koeficientas

    private void OnTriggerEnter(Collider other)
    {
        // Jei objektas turi NavMeshAgent (pvz., NPC), sulėtink jį
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed *= slowMultiplier;
        }

        // Jei objektas yra žaidėjas, sulėtink jo judėjimą
        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlow(slowMultiplier);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Jei objektas turi NavMeshAgent, atkurk jo greitį
        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed /= slowMultiplier;
        }

        // Jei objektas yra žaidėjas, atkurk jo greitį
        FirstPersonMovement playerMovement = other.GetComponent<FirstPersonMovement>();
        if (playerMovement != null)
        {
            playerMovement.RemoveSlow();
        }
    }
}
