using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GraveDigging : MonoBehaviour
{
    public Camera mainCamera;
    public Terrain terrain;
    public GameObject keyPrefab;
    public GameObject digEffectPrefab; // Particle efektas
    public Transform digSpotsParent;
    public Text keyListText; // UI Tekstas raktų sąrašui
    public int maxDiggingSteps = 5;  
    public float maxDepth = 0.2f;    
    public int digRadius = 2;        
    public float digAnimationDuration = 2.0f; 

    private List<Vector3> diggableSpots = new List<Vector3>();
    private Dictionary<Vector3, int> digProgress = new Dictionary<Vector3, int>();
    private Dictionary<Vector3, float> originalHeights = new Dictionary<Vector3, float>();
    private float[,] originalTerrainHeights;
    private bool isDigging = false;
    private int maxKeys = 3; // Maksimalus raktų skaičius
    private HashSet<Vector3> keySpots = new HashSet<Vector3>();
    private HashSet<Vector3> collectedKeys = new HashSet<Vector3>(); // Surinkti raktai  

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
        SelectRandomKeySpots();
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

        isDigging = true;

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

        if (digEffectPrefab != null)
        {
            Vector3 effectPosition = position + Vector3.up * 0.5f;
            GameObject effect = Instantiate(digEffectPrefab, effectPosition, Quaternion.Euler(-90, 0, 0));
            Destroy(effect, 2f);
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

        isDigging = false;

        if (digProgress[position] >= maxDiggingSteps && keySpots.Contains(position))
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
        Debug.Log("Raktas atsirado ant DigSpot: " + position);
    }

    public void CollectKey(Vector3 position)
    {
        // Suvienodiname `y` koordinatę, kad išvengtume neatitikimų
        position.y = 0f;

        bool alreadyCollected = collectedKeys.Any(collected =>
            Mathf.Approximately(collected.x, position.x) &&
            Mathf.Approximately(collected.z, position.z)
        );

        if (alreadyCollected)
        {
            Debug.LogWarning("⚠ Raktas iš šios vietos jau buvo surinktas: " + position);
            return;
        }

        collectedKeys.Add(position);
        Debug.Log("✅ Raktas įtrauktas į surinktus: " + position);

        UpdateKeyListUI(); // Atnaujina UI tekstą
    }




    void SaveInitialTerrainHeights()
    {
        TerrainData terrainData = terrain.terrainData;
        originalTerrainHeights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);

        Vector3 terrainPos = terrain.transform.position;

        foreach (Transform child in digSpotsParent)
        {
            Vector3 position = child.position;
            diggableSpots.Add(position);
            digProgress[position] = 0;
            originalHeights[position] = terrainData.GetHeights(Mathf.RoundToInt((position.x - terrainPos.x) / terrainData.size.x * terrainData.heightmapResolution), Mathf.RoundToInt((position.z - terrainPos.z) / terrainData.size.z * terrainData.heightmapResolution), 1, 1)[0, 0];
        }
    }

    void SelectRandomKeySpots()
    {
        keySpots = new HashSet<Vector3>(diggableSpots.OrderBy(x => Random.value).Take(maxKeys));
        Debug.Log("Raktai bus šiose vietose:");
        UpdateKeyListUI();
    }

    void UpdateKeyListUI()
    {
        if (keyListText == null)
        {
            Debug.LogError("❌ KeyListText UI nėra priskirtas! Įsitikink, kad jis yra Inspector'iuje.");
            return;
        }

        keyListText.text = "Raktai bus šiose vietose:\n";

        foreach (var spot in keySpots)
        {
            string spotName = GetSpotName(spot);

            // Naujas lyginimas, ignoruojant `y` koordinatę
            bool isCollected = collectedKeys.Any(collected =>
                Mathf.Approximately(collected.x, spot.x) &&
                Mathf.Approximately(collected.z, spot.z)
            );

            if (isCollected)
            {
                keyListText.text += "<color=#808080>- " + spotName + " (surinkta)</color>\n"; // Pilka spalva
                Debug.Log("🔹 UI atnaujinta: " + spotName + " dabar pilkas.");
            }
            else
            {
                keyListText.text += "- " + spotName + "\n";
            }
        }
    }





    string GetSpotName(Vector3 position)
    {
        foreach (Transform child in digSpotsParent)
        {
            if (Vector3.Distance(child.position, position) < 0.1f)
            {
                Debug.Log("🔎 Raktas rastas: " + child.name + " (pozicija: " + position + ")");
                return child.name;
            }
        }

        Debug.LogWarning("⚠ Raktas nerastas pagal poziciją: " + position);
        return position.ToString(); // Jei nepavyksta surasti, rodom poziciją
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

    // ŠIUOS METODUS DĖK ČIA, PO ResetTerrain()
    void OnDisable()
    {
        ResetTerrain(); // Atstato terrainą prieš išjungiant skriptą (arba išeinant iš Play Mode)
    }

    void OnApplicationQuit()
    {
        ResetTerrain(); // Papildomas saugiklis, jei Unity nepašaukia OnDisable
    }
}