using UnityEngine;

public class StringMapSpawner : MonoBehaviour
{
    [Header("배경 스프라이트 (맵 전체 영역)")]
    public SpriteRenderer background;   // background SpriteRenderer

    [Header("CSV 파일 (WALL, ENEMY, 0)")]
    public TextAsset csvFile;

    [Header("프리팹")]
    public GameObject wallPrefab;
    public GameObject enemyPrefab;      // ENEMY용
    public GameObject enemyTopPrefab; 

    private string[,] mapData;

    void Start()
    {
        LoadCSV();
        SpawnMapOnBackground();
    }

    void LoadCSV()
    {
        string[] lines = csvFile.text.Trim().Split('\n');

        int rows = lines.Length;
        int cols = lines[0].Trim().Replace("\r", "").Split(',').Length;

        mapData = new string[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            string line = lines[y].Trim().Replace("\r", "");
            string[] tokens = line.Split(',');

            for (int x = 0; x < cols; x++)
            {
                string value = tokens[x].Trim();

                // 빈칸이면 자동 0
                if (string.IsNullOrEmpty(value))
                    value = "0";

                mapData[y, x] = value;
            }
        }
    }

    void SpawnMapOnBackground()
    {
        int rows = mapData.GetLength(0);
        int cols = mapData.GetLength(1);

        Bounds bgBounds = background.bounds;
        Vector3 bottomLeft = bgBounds.min;
        float bgWidth = bgBounds.size.x;
        float bgHeight = bgBounds.size.y;

        float cellWidth = bgWidth / cols;
        float cellHeight = bgHeight / rows;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                string raw = mapData[y, x];
                string token = raw.Trim().ToUpperInvariant();

                if (string.IsNullOrEmpty(token) || token == "0")
                    continue;

                GameObject prefab = null;

                switch (token)
                {
                    case "WALL":
                        prefab = wallPrefab;
                        break;

                    case "ENEMY":
                        prefab = enemyPrefab;       // 기존 일반 적
                        break;

                    case "ENEMY_TOP":
                        prefab = enemyTopPrefab;   // 새 타입
                        break;

                    default:
                        continue;
                }

                if (prefab == null) continue;

                float centerX = bottomLeft.x + cellWidth * (x + 0.5f);
                float centerY = bottomLeft.y + cellHeight * (y + 0.5f);
                Vector3 spawnPos = new Vector3(centerX, centerY, 0f);

                GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

                // 셀 크기에 맞게 스케일 조정 (SpriteRenderer 기준)
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Vector2 spriteSize = sr.bounds.size;

                    if (spriteSize.x > 0 && spriteSize.y > 0)
                    {
                        float scaleX = cellWidth / spriteSize.x;
                        float scaleY = cellHeight / spriteSize.y;

                        obj.transform.localScale = new Vector3(
                            obj.transform.localScale.x * scaleX,
                            obj.transform.localScale.y * scaleY,
                            obj.transform.localScale.z
                        );
                    }
                }
            }
        }
    }
}
