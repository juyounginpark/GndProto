using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class DashEnemy : MonoBehaviour
{
    [Header("이동 설정")]
    public float riseSpeed = 5f;
    public float offsetFromTop = 1.5f;

    [Header("공격 설정")]
    public float holdDuration = 1.0f;
    public float dashSpeed = 30f;
    public float destroyOffset = 3f;

    private Transform cam;
    private enum State { Idle, Rising, Holding, Dashing }
    private State state = State.Idle;
    private float timer;
    private bool isDead = false;
    private EnemyHealth health;

    void Start()
    {
        cam = Camera.main.transform;
        timer = holdDuration;
        health = GetComponent<EnemyHealth>();
        // 배경 앞으로 튀어나오게 Z축 고정
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
    }

    void Update()
    {
        if (cam == null) return;

        float camHalfHeight = Camera.main.orthographicSize;
        float camTopY = cam.position.y + camHalfHeight;
        float camBottomY = cam.position.y - camHalfHeight;

        // --- 1. 대기 상태 (화면 아래에 있을 때) ---
        if (state == State.Idle)
        {
            // 화면 안으로 들어왔는지 체크 (아래쪽 경계 + 1f)
            if (transform.position.y > camBottomY - 1f)
            {
                state = State.Rising;
                // 청크에서 분리하여 독립적으로 이동
                transform.SetParent(null);
            }
            return; // Idle 상태에서는 절대 삭제하지 않고 리턴
        }

        // --- 2. 활동 상태 ---
        float targetY = camTopY - offsetFromTop;

        switch (state)
        {
            case State.Rising:
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
                transform.position = pos;

                if (Mathf.Abs(pos.y - targetY) < 0.5f) state = State.Holding;
                break;

            case State.Holding:
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
                timer -= Time.deltaTime;
                if (timer <= 0f) state = State.Dashing;
                break;

            case State.Dashing:
                // 돌진
                transform.Translate(Vector3.down * dashSpeed * Time.deltaTime, Space.World);

                // ★ 중요: 돌진 중일 때만 삭제 체크를 합니다!
                if (transform.position.y < camBottomY - destroyOffset)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }
    
    [Header("피격 설정")]
    public float bounceForce = 15f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Player"))
        {
            PlayerGuard guard = other.GetComponent<PlayerGuard>();

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

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;

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