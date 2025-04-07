using UnityEngine;

public class Svirtis : MonoBehaviour
{
    public Kolona kolona;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnMouseDown()
    {
        if (kolona != null)
        {
            kolona.TriggerColumn(); // iškviečia koloną
            animator.SetTrigger("SvirtisDown"); // nusilenkia kai paspaudžiam
        }
    }

    // Šitą iškvies kolona, kai baigs kilti
    public void LiftLever()
    {
        Debug.Log("Triggerinam SvirtisUp!");
        animator.SetTrigger("SvirtisUp");
    }
}

