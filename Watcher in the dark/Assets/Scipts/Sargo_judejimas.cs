using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RandomPatrol : MonoBehaviour
{
    [Header("Patruliavimas ir persekiojimas")]
    public Transform[] waypoints;
    public float waitTime = 2f;
    public Transform player;
    public float sightRange = 10f;
    public float chaseSpeed = 5f;
    public float patrolSpeed = 2f;
    public LayerMask obstacleMask;

    [Header("Muzika")]
    public AudioClip chaseMusic;
    public AudioClip normalMusic;
    public float fadeDuration = 2f;
    private AudioSource musicSource;

    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false;
    private bool isChasing = false;

    private float slowMultiplier = 1f;
    private Coroutine timedChaseCoroutine;
    private Coroutine chaseUpdateCoroutine;

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

        if (normalMusic != null)
        {
            musicSource.clip = normalMusic;
            musicSource.loop = true;
            musicSource.volume = 1f;
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
        UpdateSpeed();
    }

    public void StartTimedChase()
    {
        if (timedChaseCoroutine != null)
            StopCoroutine(timedChaseCoroutine);
        timedChaseCoroutine = StartCoroutine(TimedChaseRoutine());

        if (chaseUpdateCoroutine != null)
            StopCoroutine(chaseUpdateCoroutine);
        chaseUpdateCoroutine = StartCoroutine(UpdateChaseDestination());

        // Pradedam fade į persekiojimo muziką
        if (chaseMusic != null && musicSource != null)
        {
            StartCoroutine(FadeToMusic(chaseMusic));
        }
    }

    IEnumerator TimedChaseRoutine()
    {
        Debug.Log("🚨 Pradedam gaudymą 1 minutę!");
        isChasing = true;
        UpdateSpeed();

        float chaseTime = 10f;
        float elapsedTime = 0f;

        while (elapsedTime < chaseTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("✅ 1 minutė baigėsi, grįžtam į patruliavimą.");
        isChasing = false;
        UpdateSpeed();
        MoveToNextWaypoint();
        timedChaseCoroutine = null;

        if (chaseUpdateCoroutine != null)
        {
            StopCoroutine(chaseUpdateCoroutine);
            chaseUpdateCoroutine = null;
            sightRange += 5;
        }

        // Grįžtam prie ramios muzikos
        if (normalMusic != null && musicSource != null)
        {
            StartCoroutine(FadeToMusic(normalMusic));
        }
    }

    IEnumerator UpdateChaseDestination()
    {
        while (isChasing)
        {
            if (player != null)
            {
                agent.SetDestination(player.position);

                float distance = Vector3.Distance(transform.position, player.position);
                float waitTime;

                if (distance < 5f)
                {
                    waitTime = 0.2f;
                }
                else if (distance < 15f)
                {
                    waitTime = 0.5f;
                }
                else
                {
                    waitTime = 1.0f;
                }

                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    IEnumerator FadeToMusic(AudioClip newClip)
    {
        if (musicSource.isPlaying)
        {
            // Fade out
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        // Change clip
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 1f;
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
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
}
