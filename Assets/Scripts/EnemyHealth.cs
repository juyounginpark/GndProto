using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public int maxHealth = 2;
    public int currentHealth;

    [Header("체력바 설정")]
    public Vector3 healthBarOffset = new Vector3(0, -0.6f, 0);
    public Vector2 healthBarSize = new Vector2(0.8f, 0.1f);
    public Color bgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Color healthColor = Color.green;
    public Color lowHealthColor = Color.red;

    private GameObject healthBarBg;
    private GameObject healthBarFill;
    private SpriteRenderer fillRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        CreateHealthBar();
    }

    void LateUpdate()
    {
        // 체력바가 적을 따라다니도록 (적의 스케일에 영향받지 않음)
        if (healthBarBg != null)
        {
            healthBarBg.transform.position = transform.position + healthBarOffset;
        }
    }

    void CreateHealthBar()
    {
        // 배경 바 (적의 자식이 아닌 독립 오브젝트)
        healthBarBg = new GameObject("HealthBarBg");
        healthBarBg.transform.position = transform.position + healthBarOffset;
        healthBarBg.transform.localScale = new Vector3(healthBarSize.x, healthBarSize.y, 1f);

        SpriteRenderer bgRenderer = healthBarBg.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateSquareSprite();
        bgRenderer.color = bgColor;
        bgRenderer.sortingOrder = 100;

        // 체력 바 (전경)
        healthBarFill = new GameObject("HealthBarFill");
        healthBarFill.transform.SetParent(healthBarBg.transform);
        healthBarFill.transform.localPosition = Vector3.zero;
        healthBarFill.transform.localScale = Vector3.one;

        fillRenderer = healthBarFill.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = CreateSquareSprite();
        fillRenderer.color = healthColor;
        fillRenderer.sortingOrder = 101;

        UpdateHealthBar();
    }

    Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        float ratio = (float)currentHealth / maxHealth;
        healthBarFill.transform.localScale = new Vector3(ratio, 1f, 1f);

        // 왼쪽 정렬 (피벗이 중앙이므로 위치 조정)
        healthBarFill.transform.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0f);

        // 체력 낮으면 색상 변경
        if (ratio <= 0.5f)
        {
            fillRenderer.color = lowHealthColor;
        }
        else
        {
            fillRenderer.color = healthColor;
        }
    }

    void Die()
    {
        // 체력바 먼저 제거
        if (healthBarBg != null) Destroy(healthBarBg);

        // 사망 처리 - 각 적 스크립트의 사망 로직 호출
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.OnDeath();
            return;
        }

        EnemyTop enemyTop = GetComponent<EnemyTop>();
        if (enemyTop != null)
        {
            enemyTop.OnDeath();
            return;
        }

        DashEnemy dashEnemy = GetComponent<DashEnemy>();
        if (dashEnemy != null)
        {
            dashEnemy.OnDeath();
            return;
        }

        // 기본 사망 처리
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // 적이 삭제될 때 체력바도 함께 삭제
        if (healthBarBg != null) Destroy(healthBarBg);
    }
}
