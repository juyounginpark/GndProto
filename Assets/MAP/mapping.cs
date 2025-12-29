using UnityEngine;
using System.Collections.Generic;

public class InfiniteMapSpawner : MonoBehaviour
{
    [Header("패턴 CSV 파일들")]
    public TextAsset[] patternFiles;

    [Header("화면 맞춤 설정")]
    public int cols = 5;
    public bool fitToScreenWidth = true;

    [Header("생성 설정")]
    public float spawnThreshold = 20f;
    public float destroyDistance = 30f;

    [Header("토큰-프리팹 매핑")]
    public TokenPrefabEntry[] tokenPrefabTable;

    private float cellSize;
    private Dictionary<string, GameObject> tokenToPrefab;
    private Dictionary<string, int> tokenToHealth;
    private float nextSpawnY;
    private float cameraY;              // 카메라 고정 y좌표
    private Queue<GameObject> activeChunks = new Queue<GameObject>();

    void Start()
    {
        cameraY = Camera.main.transform.position.y;

        if (cols <= 0) cols = 5;

        // 토큰-프리팹 딕셔너리 초기화
        tokenToPrefab = new Dictionary<string, GameObject>();
        tokenToHealth = new Dictionary<string, int>();
        foreach (var entry in tokenPrefabTable)
        {
            if (!string.IsNullOrEmpty(entry.token) && entry.prefab != null)
            {
                string key = entry.token.Trim().ToUpperInvariant();
                tokenToPrefab[key] = entry.prefab;
                tokenToHealth[key] = entry.health;
            }
        }

        CalculateCellSize();
        nextSpawnY = 0f;

        SpawnRandomPattern();
    }

    void Update()
    {
        float scrollY = CameraFollow.ScrollOffset;

        // 새 패턴 생성 체크 (스크롤 오프셋 기준)
        if (scrollY - spawnThreshold < nextSpawnY)
        {
            SpawnRandomPattern();
        }

        // 모든 청크를 위로 이동 (낙하 효과)
        float speed = CameraFollow.CurrentSpeed;
        foreach (GameObject chunk in activeChunks)
        {
            if (chunk != null)
            {
                chunk.transform.position += Vector3.up * speed * Time.deltaTime;
            }
        }

        // 화면 위로 벗어난 청크 삭제
        if (activeChunks.Count > 0)
        {
            GameObject oldestChunk = activeChunks.Peek();
            if (oldestChunk != null && oldestChunk.transform.position.y > cameraY + destroyDistance)
            {
                Destroy(activeChunks.Dequeue());
            }
        }
    }

    void CalculateCellSize()
    {
        if (fitToScreenWidth)
        {
            float screenHeightUnits = Camera.main.orthographicSize * 2f;
            float screenWidthUnits = screenHeightUnits * Camera.main.aspect;
            cellSize = screenWidthUnits / cols;
        }
        else
        {
            cellSize = 1f;
        }
    }

    void SpawnRandomPattern()
    {
        if (patternFiles.Length == 0) return;

        int idx = Random.Range(0, patternFiles.Length);
        TextAsset csv = patternFiles[idx];

        // 청크를 카메라 y좌표 기준 아래에 생성 (스크롤 오프셋 적용)
        float spawnWorldY = cameraY + nextSpawnY - CameraFollow.ScrollOffset;

        GameObject chunk = new GameObject($"MapChunk_{nextSpawnY:F1}");
        chunk.transform.position = new Vector3(0, spawnWorldY, 0);

        float patternHeight = GenerateObjectsInChunk(chunk, csv);

        activeChunks.Enqueue(chunk);
        nextSpawnY -= patternHeight;
    }

    float GenerateObjectsInChunk(GameObject chunkParent, TextAsset csv)
    {
        string cleanText = csv.text.Replace("\r", "");
        string[] lines = cleanText.Split('\n');
        int rows = lines.Length;

        float mapWidth = cols * cellSize;
        float startX = -mapWidth / 2f;

        for (int r = 0; r < rows; r++)
        {
            string line = lines[r].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] tokens = line.Split(',');

            for (int c = 0; c < tokens.Length && c < cols; c++)
            {
                string token = tokens[c].Trim().ToUpperInvariant();

                if (string.IsNullOrEmpty(token) || token == "0") continue;

                float posX = startX + (c * cellSize) + (cellSize * 0.5f);
                float posY = -(r * cellSize) - (cellSize * 0.5f);

                GameObject prefab = GetPrefabByToken(token);

                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, chunkParent.transform);
                    obj.transform.localPosition = new Vector3(posX, posY, -5f);

                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        obj.transform.localScale = Vector3.one;
                        Vector2 spriteSize = sr.bounds.size;
                        if (spriteSize.x > 0 && spriteSize.y > 0)
                        {
                            obj.transform.localScale = new Vector3(cellSize / spriteSize.x, cellSize / spriteSize.y, 1f);
                        }
                    }

                    // 체력 설정
                    EnemyHealth enemyHealth = obj.GetComponent<EnemyHealth>();
                    if (enemyHealth != null && tokenToHealth.TryGetValue(token, out int health))
                    {
                        enemyHealth.maxHealth = health;
                        enemyHealth.currentHealth = health;
                    }
                }
            }
        }
        return rows * cellSize;
    }

    GameObject GetPrefabByToken(string token)
    {
        string key = token.Trim().ToUpperInvariant();

        if (tokenToPrefab.TryGetValue(key, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }
}
