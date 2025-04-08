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
        Debug.Log("🔑 Raktų dalis paimta: " + gameObject.name);

        GraveDigging graveDigging = Object.FindFirstObjectByType<GraveDigging>();
        if (graveDigging != null)
            graveDigging.CollectKey(keyPosition);

        RandomPatrol patrol = Object.FindFirstObjectByType<RandomPatrol>();
        if (patrol != null)
            patrol.StartTimedChase();

        Destroy(gameObject);
    }
}
