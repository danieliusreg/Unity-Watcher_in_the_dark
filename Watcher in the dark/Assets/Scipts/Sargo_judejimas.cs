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

    [Header("Muzika")]
    public AudioClip chaseMusic;
    public AudioClip normalMusic;
    private AudioSource musicSource;

    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false;
    private bool isChasing = false;
    private Vector3 lastSeenPosition;

    private float slowMultiplier = 1f;
    private Coroutine timedChaseCoroutine;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        UpdateSpeed();
        MoveToNextWaypoint();

        // Muzikos šaltinis
        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        // Paleidžiam ramų garso takelį
        if (normalMusic != null)
        {
            musicSource.clip = normalMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            StartChasing();
        }
        else if (isChasing && timedChaseCoroutine == null)
        {
            if (agent.remainingDistance < 0.5f)
            {
                isChasing = false;
                UpdateSpeed();
                MoveToNextWaypoint();
            }
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting && !isChasing)
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
        if (timedChaseCoroutine != null) return;

        isChasing = true;
        agent.destination = player.position;
        lastSeenPosition = player.position;
        UpdateSpeed();
    }

    public void StartTimedChase()
    {
        if (timedChaseCoroutine != null)
        {
            StopCoroutine(timedChaseCoroutine);
        }

        // Paleidžiam gaudymo muziką
        if (chaseMusic != null && musicSource != null)
        {
            musicSource.clip = chaseMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        timedChaseCoroutine = StartCoroutine(TimedChaseRoutine());
    }

    IEnumerator TimedChaseRoutine()
    {
        Debug.Log("🚨 Pradedam gaudymą 1 minutę!");
        isChasing = true;
        UpdateSpeed();

        float chaseTime = 60f;
        float elapsedTime = 0f;

        while (elapsedTime < chaseTime)
        {
            if (player != null)
            {
                agent.destination = player.position;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("✅ 1 minutė baigėsi, grįžtam į patruliavimą.");
        isChasing = false;
        UpdateSpeed();
        MoveToNextWaypoint();
        timedChaseCoroutine = null;

        // Grįžtam prie ramios muzikos
        if (normalMusic != null && musicSource != null)
        {
            musicSource.clip = normalMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

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

    void OnDrawGizmosSelected()
    {
        if (player == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
