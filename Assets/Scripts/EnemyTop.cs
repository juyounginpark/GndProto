using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyTop : MonoBehaviour
{
    [Header("이동 설정")]
    public float riseSpeed = 5f;
    public float leaveSpeed = 10f;
    public float offsetFromTop = 1.5f;
    public float destroyOffset = 3f;

    [Header("공격 설정")]
    public GameObject bulletPrefab;
    public float attackDelay = 0.5f;
    public float afterAttackDelay = 0.5f;

    private Transform cam;
    private enum State { Idle, Rising, Holding, Leaving }
    private State state = State.Idle;

    private float timer;
    private bool hasFired = false;
    private bool isDead = false;
    private EnemyHealth health;

    void Start()
    {
        cam = Camera.main.transform;
        health = GetComponent<EnemyHealth>();
        // 배경 앞으로 튀어나오게 Z축 고정
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
    }

    void Update()
    {
        if (cam == null) return;

        float camTopY = cam.position.y + Camera.main.orthographicSize;
        float camBottomY = cam.position.y - Camera.main.orthographicSize;

        // --- 1. 대기 상태 ---
        if (state == State.Idle)
        {
            // 화면 안으로 들어오면 시작
            if (transform.position.y > camBottomY - 1f)
            {
                state = State.Rising;
                // 청크에서 분리하여 독립적으로 이동
                transform.SetParent(null);
            }
            return; // 절대 삭제하지 않음
        }

        // --- 2. 활동 상태 ---
        float targetY = camTopY - offsetFromTop;

        switch (state)
        {
            case State.Rising:
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
                transform.position = pos;

                if (Mathf.Abs(pos.y - targetY) < 0.5f)
                {
                    state = State.Holding;
                    timer = attackDelay;
                }
                break;

            case State.Holding:
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                
                timer -= Time.deltaTime;
                if (timer <= 0f && !hasFired)
                {
                    FireBullet();
                    hasFired = true;
                    timer = afterAttackDelay;
                }
                if (hasFired && timer <= 0f) state = State.Leaving;
                break;

            case State.Leaving:
                transform.Translate(Vector3.up * leaveSpeed * Time.deltaTime, Space.World);

                // ★ 중요: 퇴장 중일 때만 삭제 체크!
                if (transform.position.y > camTopY + destroyOffset)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    void FireBullet()
    {
        if (bulletPrefab != null)
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }

    [Header("피격 설정")]
    public float bounceForce = 20f; // 튕겨나가는 힘

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerGuard guard = other.GetComponent<PlayerGuard>();

            // 가드 성공 시
            if (guard != null && guard.IsGuarding)
            {
                Debug.Log("가드 성공! 튕겨냅니다!");
                BounceAway(other);
            }
            else
            {
                // 플레이어에게 닿으면 1 데미지
                health.TakeDamage(1);
            }
        }
    }

    void BounceAway(Collider2D other)
    {
        isDead = true;

        // 1. 더 이상 공격/이동 로직이 돌지 않도록 스크립트 비활성화
        this.enabled = false;

        // 2. 다시 충돌하지 않도록 콜라이더 끄기
        GetComponent<Collider2D>().enabled = false;

        // 3. 물리력 가하기
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            Vector2 dir = (transform.position - other.transform.position).normalized;
            rb.AddForce(dir * bounceForce, ForceMode2D.Impulse);
            rb.angularVelocity = Random.Range(-300f, 300f);
        }

        Destroy(gameObject, 2f);
    }

    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject);
    }
}