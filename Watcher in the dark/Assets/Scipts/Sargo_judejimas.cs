
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum GuardState
{
    Patrol,
    Chase,
    InvestigateSound,
    Wait,
    Alert // nauja būsena – kai visada seka žaidėją, pvz. po rakto paėmimo
}

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

    [Header("Girdėjimas")]
    public float hearingRadius = 8f;
    public LayerMask hearingMask;

    [Header("Muzika")]
    public AudioClip chaseMusic;
    public AudioClip normalMusic;
    public float fadeDuration = 2f;

    private AudioSource musicSource;
    private NavMeshAgent agent;
    private int currentWaypointIndex = -1;
    private bool isWaiting = false;

    private float slowMultiplier = 1f;
    private Coroutine timedChaseCoroutine;
    private Coroutine chaseUpdateCoroutine;

    private Vector3 lastHeardPosition;
    private bool heardSound = false;

    private GuardState currentState = GuardState.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        UpdateSpeed();
        MoveToNextWaypoint();

        musicSource = GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

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
        switch (currentState)
        {
            case GuardState.Patrol:
                PatrolUpdate();
                break;
            case GuardState.Chase:
                ChaseUpdate();
                break;
            case GuardState.InvestigateSound:
                InvestigateSoundUpdate();
                break;
            case GuardState.Alert:
                ChaseUpdate(); // seka žaidėją nuolat
                break;
            case GuardState.Wait:
                break;
        }

        if (CanSeePlayer() && currentState != GuardState.Chase && currentState != GuardState.Alert)
        {
            StartChasing();
        }
    }

    void PatrolUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isWaiting)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    void ChaseUpdate()
    {
        if (agent.remainingDistance < 0.5f && timedChaseCoroutine == null)
        {
            SetState(GuardState.Patrol);
            MoveToNextWaypoint();
        }
    }

    void InvestigateSoundUpdate()
    {
        if (agent.remainingDistance < 0.5f)
        {
            heardSound = false;
            SetState(GuardState.Patrol);
            MoveToNextWaypoint();
        }
    }

    IEnumerator WaitAndMove()
    {
        isWaiting = true;
        SetState(GuardState.Wait);
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        SetState(GuardState.Patrol);
        MoveToNextWaypoint();
    }

    void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0 || currentState == GuardState.Chase || currentState == GuardState.Alert)
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
        if (player == null) return false;

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
        SetState(GuardState.Chase);
        agent.destination = player.position;
    }

    public void TriggerAlert()
    {
        if (timedChaseCoroutine != null)
            StopCoroutine(timedChaseCoroutine);
        timedChaseCoroutine = StartCoroutine(AlertRoutine());

        if (chaseUpdateCoroutine != null)
            StopCoroutine(chaseUpdateCoroutine);
        chaseUpdateCoroutine = StartCoroutine(UpdateChaseDestination());

        if (chaseMusic != null && musicSource != null)
        {
            StartCoroutine(FadeToMusic(chaseMusic));
        }
    }

    IEnumerator AlertRoutine()
    {
        Debug.Log("⚠️ Aliarmas! Sargas visą minutę seka žaidėją!");
        SetState(GuardState.Alert);

        float alertTime = 60f;
        float elapsedTime = 0f;

        while (elapsedTime < alertTime)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("🟢 Aliarmas baigėsi, grįžtam į patruliavimą.");
        SetState(GuardState.Patrol);
        MoveToNextWaypoint();
        timedChaseCoroutine = null;

        if (chaseUpdateCoroutine != null)
        {
            StopCoroutine(chaseUpdateCoroutine);
            chaseUpdateCoroutine = null;
        }

        if (normalMusic != null && musicSource != null)
        {
            StartCoroutine(FadeToMusic(normalMusic));
        }
    }

    IEnumerator UpdateChaseDestination()
    {
        while (currentState == GuardState.Chase || currentState == GuardState.Alert)
        {
            if (player != null)
            {
                agent.SetDestination(player.position);

                float distance = Vector3.Distance(transform.position, player.position);
                float waitTime = distance < 5f ? 0.2f : (distance < 15f ? 0.5f : 1.0f);

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
            float startVolume = musicSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }
            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.Play();

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
        agent.speed = (currentState == GuardState.Chase || currentState == GuardState.Alert ? chaseSpeed : patrolSpeed) * slowMultiplier;
    }

    public void OnFootstepHeard(Vector3 soundPosition)
    {
        float distance = Vector3.Distance(transform.position, soundPosition);
        if (distance <= hearingRadius && currentState != GuardState.Chase && currentState != GuardState.Alert)
        {
            heardSound = true;
            lastHeardPosition = soundPosition;
            Debug.Log("👂 Sargas išgirdo žingsnius! Eina tikrinti.");
            SetState(GuardState.InvestigateSound);
        }
    }

    void SetState(GuardState newState)
    {
        currentState = newState;
        Debug.Log($"🧠 Nauja būsena: {currentState}");

        switch (newState)
        {
            case GuardState.Patrol:
                UpdateSpeed();
                break;
            case GuardState.Chase:
            case GuardState.Alert:
                UpdateSpeed();
                break;
            case GuardState.InvestigateSound:
                agent.SetDestination(lastHeardPosition);
                UpdateSpeed();
                break;
            case GuardState.Wait:
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRadius);
    }
}
