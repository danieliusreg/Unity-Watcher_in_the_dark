using UnityEngine;

public class MudZone : MonoBehaviour
{
    public float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        {
            Debug.Log("Agent entered mud: " + other.name);
            agent.speed *= slowMultiplier;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out UnityEngine.AI.NavMeshAgent agent))
        {
            Debug.Log("Agent exited mud: " + other.name);
            agent.speed /= slowMultiplier;
        }
    }
}
