using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RandomPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime = 2f;
    public Transform player;
    public float sightRange = 10f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public LayerMask playerLayer;
    public LayerMask obstacleMask;

    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false;
    private bool isChasing = false;
    private Vector3 lastSeenPosition;

    private float slowMultiplier = 1f; // Sulėtėjimo koeficientas (1f = normalus greitis)

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        UpdateSpeed();
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
            if (agent.remainingDistance < 0.5f)
            {
                isChasing = false;
                UpdateSpeed();
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

        if (distanceToPlayer < sightRange)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            {
                return true;
            }
        }
        return false;
    }

    void StartChasing()
    {
        isChasing = true;
        agent.destination = player.position;
        lastSeenPosition = player.position;
        UpdateSpeed();
    }

    // === Nauja dalis: sulėtėjimo palaikymas ===

    public void ApplySlow(float multiplier)
    {
        slowMultiplier = multiplier;
        UpdateSpeed();
    }

    public void RemoveSlow()
    {
        slowMultiplier = 1f;
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        agent.speed = (isChasing ? chaseSpeed : patrolSpeed) * slowMultiplier;
    }
}
