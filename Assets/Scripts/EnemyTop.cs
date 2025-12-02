using UnityEngine;

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

    void Start()
    {
        cam = Camera.main.transform;
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
        // 원본이면 무시 (기존 코드 유지)
        // if (!isClone) return; 

        if (other.CompareTag("Player"))
        {
            PlayerGuard guard = other.GetComponent<PlayerGuard>();

            // 가드 성공 시
            if (guard != null && guard.IsGuarding)
            {
                Debug.Log("🛡️ 가드 성공! 튕겨냅니다!");

                // 1. 더 이상 공격/이동 로직이 돌지 않도록 스크립트 비활성화
                this.enabled = false; 

                // 2. 다시 충돌하지 않도록 콜라이더 끄기 (선택 사항)
                GetComponent<Collider2D>().enabled = false;

                // 3. 물리력 가하기
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 현재 움직임 멈춤
                    rb.linearVelocity = Vector2.zero; 

                    // 플레이어 반대 방향 계산 (내 위치 - 플레이어 위치)
                    Vector2 dir = (transform.position - other.transform.position).normalized;
                    
                    // 힘 가하기 (Impulse: 순간적인 힘)
                    rb.AddForce(dir * bounceForce, ForceMode2D.Impulse);
                    
                    // 뱅글뱅글 돌게 회전력 추가 (타격감 상승)
                    rb.angularVelocity = Random.Range(-300f, 300f);
                }

                // 4. 화면 밖으로 날아가는 모습 보여준 뒤 2초 후 삭제
                Destroy(gameObject, 2f);
            }
            else
            {
                // 가드 실패 (플레이어 피격 등)
                Debug.Log("💥 플레이어 피격!");
                Destroy(gameObject); // 적은 그냥 자폭
            }
        }
    }
}