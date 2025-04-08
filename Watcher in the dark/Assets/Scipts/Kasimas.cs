using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class GraveDigging : MonoBehaviour
{
    public Camera mainCamera;
    public Terrain terrain;
    public Transform player;
    public GameObject[] keyPartPrefabs; // <-- 3 raktų dalys
    public GameObject digEffectPrefab;
    public Transform digSpotsParent;
    public Text keyListText;

    public float digDistanceThreshold = 3f;
    public int maxDiggingSteps = 5;
    public float maxDepth = 0.2f;
    public int digRadius = 2;
    public float digAnimationDuration = 2.0f;
    public AudioClip digSound;

    private AudioSource audioSource;
    private List<Vector3> diggableSpots = new List<Vector3>();
    private Dictionary<Vector3, int> digProgress = new Dictionary<Vector3, int>();
    private Dictionary<Vector3, float> originalHeights = new Dictionary<Vector3, float>();
    private float[,] originalTerrainHeights;
    private bool isDigging = false;
    private int maxKeys = 3;

    private struct KeySpotInfo
    {
        public Vector3 position;
        public GameObject prefab;
    }

    private List<KeySpotInfo> keySpotInfos = new List<KeySpotInfo>();
    private HashSet<Vector3> collectedKeys = new HashSet<Vector3>();

    void Start()
    {
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned and no active terrain found!");
            return;
        }

        SaveInitialTerrainHeights();
        SelectRandomKeySpots();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
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
                        if (player != null && Vector3.Distance(player.position, spot) > digDistanceThreshold)
                        {
                            Debug.Log("⛔ Per toli nuo kasimo vietos!");
                            break;
                        }

                        StartCoroutine(DigHoleSmoothly(spot));
                        break;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
            ResetTerrain();
    }

    IEnumerator DigHoleSmoothly(Vector3 position)
    {
        if (!digProgress.ContainsKey(position) || digProgress[position] >= maxDiggingSteps)
            yield break;

        isDigging = true;

        if (digSound != null && audioSource != null)
            audioSource.PlayOneShot(digSound);

        digProgress[position]++;

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

        while (elapsedTime < digAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / digAnimationDuration;

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

        if (digProgress[position] >= maxDiggingSteps)
        {
            var info = keySpotInfos.FirstOrDefault(i => Vector3.Distance(i.position, position) < 0.1f);
            if (info.prefab != null)
                SpawnKeyPart(info);
        }
    }

    void SpawnKeyPart(KeySpotInfo info)
    {
        Vector3 keyPosition = info.position + Vector3.up * 0.2f;
        Instantiate(info.prefab, keyPosition, Quaternion.identity);
        Debug.Log("🧩 Raktų dalis atsirado: " + info.prefab.name);
    }

    public void CollectKey(Vector3 position)
    {
        position.y = 0f;

        bool alreadyCollected = collectedKeys.Any(collected =>
            Mathf.Approximately(collected.x, position.x) &&
            Mathf.Approximately(collected.z, position.z)
        );

        if (alreadyCollected)
        {
            Debug.LogWarning("⚠ Raktų dalis jau surinkta: " + position);
            return;
        }

        collectedKeys.Add(position);
        Debug.Log("✅ Raktų dalis surinkta: " + position);

        UpdateKeyListUI();

        if (collectedKeys.Count >= maxKeys)
        {
            Debug.Log("🏆 Surinktos visos raktų dalys!");
            // Gali pridėti logiką vartams atidaryti ir pan.
        }
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
            originalHeights[position] = terrainData.GetHeights(
                Mathf.RoundToInt((position.x - terrainPos.x) / terrainData.size.x * terrainData.heightmapResolution),
                Mathf.RoundToInt((position.z - terrainPos.z) / terrainData.size.z * terrainData.heightmapResolution),
                1, 1)[0, 0];
        }
    }

    void SelectRandomKeySpots()
    {
        keySpotInfos.Clear();
        var selectedSpots = diggableSpots.OrderBy(x => Random.value).Take(maxKeys).ToList();

        for (int i = 0; i < selectedSpots.Count && i < keyPartPrefabs.Length; i++)
        {
            keySpotInfos.Add(new KeySpotInfo
            {
                position = selectedSpots[i],
                prefab = keyPartPrefabs[i]
            });
        }

        UpdateKeyListUI();
    }

    void UpdateKeyListUI()
    {
        if (keyListText == null)
        {
            Debug.LogError("❌ KeyListText nėra priskirtas!");
            return;
        }

        keyListText.text = "Raktų dalys:\n";
        foreach (var spotInfo in keySpotInfos)
        {
            string spotName = GetSpotName(spotInfo.position);
            bool isCollected = collectedKeys.Any(c =>
                Mathf.Approximately(c.x, spotInfo.position.x) &&
                Mathf.Approximately(c.z, spotInfo.position.z)
            );

            if (isCollected)
                keyListText.text += $"<color=#808080>- {spotName} (surinkta)</color>\n";
            else
                keyListText.text += $"- {spotName}\n";
        }
    }

    string GetSpotName(Vector3 position)
    {
        foreach (Transform child in digSpotsParent)
        {
            if (Vector3.Distance(child.position, position) < 0.1f)
                return child.name;
        }
        return position.ToString();
    }

    void ResetTerrain()
    {
        if (terrain == null || originalTerrainHeights == null) return;

        terrain.terrainData.SetHeights(0, 0, originalTerrainHeights);
        foreach (Vector3 pos in diggableSpots)
            digProgress[pos] = 0;

        Debug.Log("🌱 Terrain atstatytas.");
    }

    void OnDisable() => ResetTerrain();
    void OnApplicationQuit() => ResetTerrain();
}
