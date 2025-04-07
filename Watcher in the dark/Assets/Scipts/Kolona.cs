using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;

public class Kolona : MonoBehaviour
{
    [Header("NavMesh komponentai")]
    public NavMeshSurface topSurface;
    public NavMeshLink topLink1;
    public NavMeshLink topLink2;
    public NavMeshObstacle blocker;
    public Svirtis svirtis;

    [Header("Nustatymai")]
    public float delayToGoUp = 120f; // Kiek sekundžių laukti prieš pakylant (pvz. 2 minutės)

    private Animator animator;
    private bool isUp = true;
    private bool isInCooldown = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetUpState(); // Pradinė būsena
    }

    public void TriggerColumn()
    {
        if (!isUp || isInCooldown) return;

        // Paleidžiam coroutine
        StartCoroutine(ColumnDownThenUp());
    }

    private IEnumerator ColumnDownThenUp()
    {
        isInCooldown = true;

        animator.SetTrigger("KolonaDown");
        isUp = false;

        // Palaukiam kol animacija pilnai nusileidžia (jei reikia, gali įdėti "yield return new WaitForSeconds()" prieš laukimą)
        yield return new WaitForSeconds(delayToGoUp);

        animator.SetTrigger("KolonaUp");
        isUp = true;

        // Laukiam kol pakils
        yield return new WaitForSeconds(1f); // čia galima koreguoti pagal animaciją

        isInCooldown = false;
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

        if (svirtis != null)
        {
            Debug.Log("Svirtis pakeliama!");
            svirtis.LiftLever(); // čia iškviečiamas trigger
        }
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

    private void SetUpState()
    {
        // Tik pasileidimo metu, kolona laikoma pakelta
        OnColumnUp();
    }
}
