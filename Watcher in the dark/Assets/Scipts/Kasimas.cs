using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraveDigging : MonoBehaviour
{
    public Camera mainCamera;
    public Terrain terrain;
    public GameObject keyPrefab;
    public GameObject digEffectPrefab; // Particle efektas
    public Transform digSpotsParent;
    public int maxDiggingSteps = 5;  
    public float maxDepth = 0.2f;    
    public int digRadius = 2;        
    public float digAnimationDuration = 2.0f; 

    private List<Vector3> diggableSpots = new List<Vector3>();
    private Dictionary<Vector3, int> digProgress = new Dictionary<Vector3, int>();
    private Dictionary<Vector3, float> originalHeights = new Dictionary<Vector3, float>();
    private float[,] originalTerrainHeights;
    private bool isDigging = false; 

    void Start()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain;
        }

        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned and no active terrain found! Assign Terrain in Inspector.");
            return;
        }

        SaveInitialTerrainHeights();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDigging) 
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPoint = hit.point;

                foreach (Vector3 spot in diggableSpots)
                {
                    if (Vector3.Distance(hitPoint, spot) < 1.5f)
                    {
                        StartCoroutine(DigHoleSmoothly(spot));
                        break;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetTerrain();
        }
    }

    IEnumerator DigHoleSmoothly(Vector3 position)
    {
        if (!digProgress.ContainsKey(position)) yield break;
        if (digProgress[position] >= maxDiggingSteps) yield break;

        isDigging = true; // Blokuoja kitus paspaudimus

        digProgress[position]++;

        if (terrain == null)
        {
            Debug.LogError("Terrain is missing! Make sure it is assigned.");
            isDigging = false;
            yield break;
        }

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;

        int terrainWidth = terrainData.heightmapResolution;
        int terrainHeight = terrainData.heightmapResolution;

        int xCenter = Mathf.RoundToInt((position.x - terrainPos.x) / terrainData.size.x * terrainWidth);
        int yCenter = Mathf.RoundToInt((position.z - terrainPos.z) / terrainData.size.z * terrainHeight);

        int digSize = digRadius * 2 + 1; 
        float[,] heights = terrainData.GetHeights(xCenter - digRadius, yCenter - digRadius, digSize, digSize);

        float stepDepth = maxDepth / maxDiggingSteps;
        float minAllowedHeight = originalHeights[position] - maxDepth;

        float elapsedTime = 0;
        float animationTime = digAnimationDuration;
        float[,] startHeights = (float[,])heights.Clone();
        float[,] targetHeights = (float[,])heights.Clone();

        for (int i = 0; i < digSize; i++)
        {
            for (int j = 0; j < digSize; j++)
            {
                targetHeights[i, j] = Mathf.Max(heights[i, j] - stepDepth, minAllowedHeight);
            }
        }

        // **Paleidžiam particle efektą**
        if (digEffectPrefab != null)
        {
            GameObject effect = Instantiate(digEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f); // Po 2 sekundžių sunaikiname efektą
        }

        while (elapsedTime < animationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / animationTime;

            for (int i = 0; i < digSize; i++)
            {
                for (int j = 0; j < digSize; j++)
                {
                    heights[i, j] = Mathf.Lerp(startHeights[i, j], targetHeights[i, j], t);
                }
            }

            terrainData.SetHeights(xCenter - digRadius, yCenter - digRadius, heights);
            yield return null;
        }

        isDigging = false; // Leidžia vėl spausti po animacijos

        Debug.Log("Kasimo progresas: " + digProgress[position] + "/" + maxDiggingSteps);

        if (digProgress[position] >= maxDiggingSteps)
        {
            SpawnKey(position);
        }
    }

    void SpawnKey(Vector3 position)
    {
        if (keyPrefab == null)
        {
            Debug.LogError("Key Prefab is missing! Assign it in the Inspector.");
            return;
        }

        Vector3 keyPosition = position;
        keyPosition.y += 0.2f;

        Instantiate(keyPrefab, keyPosition, Quaternion.identity);
        Debug.Log("Raktas atsirado ant DigSpot: " + keyPosition);
    }

    void SaveInitialTerrainHeights()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is missing! Cannot save initial heights.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        originalTerrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        Vector3 terrainPos = terrain.transform.position;

        if (digSpotsParent == null)
        {
            Debug.LogError("digSpotsParent is not assigned! Assign it in the Inspector.");
            return;
        }

        foreach (Transform child in digSpotsParent)
        {
            Vector3 position = child.position;
            diggableSpots.Add(position);
            digProgress[position] = 0;

            int x = Mathf.RoundToInt((position.x - terrainPos.x) / terrainData.size.x * terrainData.heightmapResolution);
            int y = Mathf.RoundToInt((position.z - terrainPos.z) / terrainData.size.z * terrainData.heightmapResolution);
            originalHeights[position] = terrainData.GetHeights(x, y, 1, 1)[0, 0];
        }
    }

    void ResetTerrain()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is missing! Cannot reset.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        if (originalTerrainHeights == null)
        {
            Debug.LogError("Original terrain heights not found! Terrain cannot be reset.");
            return;
        }

        terrainData.SetHeights(0, 0, originalTerrainHeights);

        foreach (Vector3 position in diggableSpots)
        {
            digProgress[position] = 0;
        }

        Debug.Log("Terrain atstatytas!");
    }
}
