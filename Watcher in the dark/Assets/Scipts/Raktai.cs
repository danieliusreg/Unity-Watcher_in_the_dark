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

        // Pataisome pasenusį metodą
        GraveDigging graveDigging = Object.FindFirstObjectByType<GraveDigging>();
        if (graveDigging != null)
        {
            Debug.Log("GraveDigging scriptas rastas, kviečiame CollectKey.");
            graveDigging.CollectKey(keyPosition); // Praneša, kad raktas paimtas
        }
        else
        {
            Debug.LogError("GraveDigging scriptas NERASTAS! Įsitikink, kad jis yra scenoje.");
        }

        Destroy(gameObject); // Raktas dingsta
    }
}
