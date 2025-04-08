using UnityEngine;

public class Key : MonoBehaviour
{
    private Vector3 keyPosition;

    void Start()
    {
        keyPosition = transform.position;
    }

    void OnMouseDown()
    {
        Debug.Log("Raktas paimtas: " + gameObject.name);

        // Raktas įtraukiamas į sąrašą
        GraveDigging graveDigging = Object.FindFirstObjectByType<GraveDigging>();
        if (graveDigging != null)
        {
            graveDigging.CollectKey(keyPosition);
        }
        else
        {
            Debug.LogError("GraveDigging scriptas NERASTAS!");
        }

        // Paleidžiam sargo gaudymo logiką
        RandomPatrol patrol = Object.FindFirstObjectByType<RandomPatrol>();
        if (patrol != null)
        {
            patrol.StartTimedChase();
        }
        else
        {
            Debug.LogError("RandomPatrol scriptas nerastas!");
        }

        Destroy(gameObject); // Raktas dingsta
    }
}
