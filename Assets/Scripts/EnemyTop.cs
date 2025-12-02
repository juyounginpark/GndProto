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
}