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

    [Header("프리팹 연결")]
    public GameObject wallPrefab;
    public GameObject enemyPrefab;      
    public GameObject enemyTopPrefab;   
    public GameObject enemyDashPrefab;  

    private float cellSize;             
    private float nextSpawnY;           
    private Transform camTransform;
    private Queue<GameObject> activeChunks = new Queue<GameObject>(); 

    void Start()
    {
        camTransform = Camera.main.transform;
        
        if (cols <= 0) cols = 5;

        CalculateCellSize();
        nextSpawnY = camTransform.position.y; 

        SpawnRandomPattern(); // 테스트를 위해 1개만 먼저 생성
    }

    void Update()
    {
        if (camTransform == null) return;

        if (camTransform.position.y - spawnThreshold < nextSpawnY)
        {
            SpawnRandomPattern();
        }

        if (activeChunks.Count > 0)
        {
            GameObject oldestChunk = activeChunks.Peek();
            if (oldestChunk.transform.position.y > camTransform.position.y + destroyDistance)
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

        GameObject chunk = new GameObject($"MapChunk_{nextSpawnY:F1}");
        chunk.transform.position = new Vector3(0, nextSpawnY, 0);

        float patternHeight = GenerateObjectsInChunk(chunk, csv);

        activeChunks.Enqueue(chunk);
        nextSpawnY -= patternHeight; 
    }

    // ★ 여기가 문제입니다! 디버깅 로그를 추가했습니다.
    float GenerateObjectsInChunk(GameObject chunkParent, TextAsset csv)
    {
        // 1. 줄바꿈 문자(\r) 완벽 제거 후 줄 단위 분리
        string cleanText = csv.text.Replace("\r", ""); 
        string[] lines = cleanText.Split('\n');
        int rows = lines.Length;

        float mapWidth = cols * cellSize;
        float startX = -mapWidth / 2f;

        Debug.Log($"[CSV 분석 시작] 파일명: {csv.name}, 총 줄 수: {rows}, 설정된 Cols: {cols}");

        for (int r = 0; r < rows; r++)
        {
            string line = lines[r].Trim(); // 앞뒤 공백 제거
            if (string.IsNullOrEmpty(line)) continue; // 빈 줄 건너뜀

            string[] tokens = line.Split(',');

            // Debug.Log($"-> {r}번째 줄 내용: {line} (칸 수: {tokens.Length})");

            for (int c = 0; c < tokens.Length && c < cols; c++)
            {
                // 2. 각 칸의 공백 완벽 제거 및 대문자 변환
                string token = tokens[c].Trim().ToUpperInvariant();

                // 빈 칸이나 0은 무시
                if (string.IsNullOrEmpty(token) || token == "0") continue;

                // Debug.Log($"   [{r},{c}] 읽은 데이터: '{token}'"); // 데이터 확인용

                float posX = startX + (c * cellSize) + (cellSize * 0.5f);
                float posY = -(r * cellSize) - (cellSize * 0.5f);

                GameObject prefab = GetPrefabByToken(token);
                
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, chunkParent.transform);
                    obj.transform.localPosition = new Vector3(posX, posY, -5f); // Z축 앞으로

                    // 스케일 조정
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
                }
                else
                {
                    // ★ 프리팹을 못 찾았을 때 경고 띄우기
                    Debug.LogWarning($"!!! 경고: '{token}'에 해당하는 프리팹을 찾지 못했습니다! 스위치문을 확인하세요.");
                }
            }
        }
        return rows * cellSize;
    }

    GameObject GetPrefabByToken(string token)
    {
        // 공백이 혹시라도 있을까봐 한 번 더 제거
        token = token.Trim(); 

        switch (token)
        {
            case "WALL": return wallPrefab;
            case "E_A": return enemyPrefab;
            case "E_B": return enemyTopPrefab;
            case "E_C": return enemyDashPrefab;
            default: 
                // 알 수 없는 문자가 들어오면 로그 출력
                // Debug.Log($"알 수 없는 토큰: {token}");
                return null;
        }
    }
}