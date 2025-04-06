using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class Kolona : MonoBehaviour
{
    [Header("NavMesh komponentai")]
    public NavMeshSurface topSurface;
    public NavMeshLink topLink1;
    public NavMeshLink topLink2;
    public NavMeshObstacle blocker;

    [Header("Animacija")]
    public float delayBetweenMoves = 5f; // kas kiek sekundžių keisti būseną

    private Animator animator;
    private bool isUp = false; // Ar šiuo metu kolona išlindusi?

    void Start()
    {
        animator = GetComponent<Animator>();
        InvokeRepeating(nameof(SwitchState), 0f, delayBetweenMoves);
    }

    void SwitchState()
    {
        if (isUp)
        {
            animator.SetTrigger("KolonaDown");
            isUp = false;
        }
        else
        {
            animator.SetTrigger("KolonaUp");
            isUp = true;
        }
    }

    // Šitos funkcijos kviečiamos iš animacijos eventų
    public void OnColumnUp()
    {
        if (blocker != null)
            blocker.carving = false;

        if (topSurface != null)
            topSurface.BuildNavMesh();

        if (topLink1 != null)
            topLink1.enabled = true;

        if (topLink2 != null)
            topLink2.enabled = true;

        Debug.Log("Kolona pakilo");
    }

    public void OnColumnDown()
    {
        if (blocker != null)
            blocker.carving = true;

        if (topSurface != null)
            topSurface.RemoveData();

        if (topLink1 != null)
            topLink1.enabled = false;

        if (topLink2 != null)
            topLink2.enabled = false;

        Debug.Log("Kolona nusileido");
    }
}
