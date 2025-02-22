using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RandomPatrol : MonoBehaviour
{
    public Transform[] waypoints; // Taškai, per kuriuos juda sargybinis
    public float waitTime = 2f;   // Kiek sekundžių laukti atvykus į tašką
    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false; // Užtikrina, kad laukimas vyksta tik vieną kartą

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    IEnumerator WaitAndMove()
    {
        isWaiting = true; // Užfiksuoja, kad laukimas vyksta
        yield return new WaitForSeconds(waitTime);
        MoveToNextWaypoint();
        isWaiting = false; // Atlaisvina laukimo būseną
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;

        int newWaypointIndex;
        do
        {
            newWaypointIndex = Random.Range(0, waypoints.Length);
        } while (newWaypointIndex == currentWaypointIndex); // Užtikrina, kad nesirinktų to paties taško iš eilės

        currentWaypointIndex = newWaypointIndex;
        agent.destination = waypoints[currentWaypointIndex].position;
    }
}
