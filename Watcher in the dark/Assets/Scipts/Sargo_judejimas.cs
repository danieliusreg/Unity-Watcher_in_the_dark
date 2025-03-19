using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RandomPatrol : MonoBehaviour
{
    public Transform[] waypoints; // Patruliavimo taškai
    public float waitTime = 2f;   // Kiek laiko laukti atvykus į tašką
    public Transform player;      // Žaidėjo objektas
    public float sightRange = 10f; // Matymo nuotolis
    public float chaseSpeed = 5f;  // Greitis gaudant žaidėją
    public float patrolSpeed = 2f; // Greitis patruliuojant
    public LayerMask playerLayer; // Žaidėjo sluoksnis
    public LayerMask obstacleMask; // Kliūčių sluoksnis

    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false; // Užtikrina, kad laukimas vyksta tik vieną kartą
    private bool isChasing = false; // Ar sargas vejasi žaidėją?
    private Vector3 lastSeenPosition; // Paskutinė vieta, kur matytas žaidėjas

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        MoveToNextWaypoint();
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            StartChasing();
        }
        else if (isChasing)
        {
            if (agent.remainingDistance < 0.5f) // Jei pasiekė paskutinę matytą žaidėjo vietą
            {
                isChasing = false;
                agent.speed = patrolSpeed;
                MoveToNextWaypoint();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    IEnumerator WaitAndMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        MoveToNextWaypoint();
        isWaiting = false;
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0 || isChasing)
            return;

        int newWaypointIndex;
        do
        {
            newWaypointIndex = Random.Range(0, waypoints.Length);
        } while (newWaypointIndex == currentWaypointIndex);

        currentWaypointIndex = newWaypointIndex;
        agent.destination = waypoints[currentWaypointIndex].position;
    }

    bool CanSeePlayer()
    {
        if (player == null)
            return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Patikrina, ar žaidėjas yra matymo ribose ir nėra paslėptas už kliūčių
        if (distanceToPlayer < sightRange)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                return true; // Žaidėjas matomas
            }
        }
        return false;
    }

    void StartChasing()
    {
        isChasing = true;
        agent.speed = chaseSpeed;
        agent.destination = player.position;
        lastSeenPosition = player.position; // Išsaugoma paskutinė matyta vieta
    }
}