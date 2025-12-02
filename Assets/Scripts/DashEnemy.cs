using UnityEngine;

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

    void Start()
    {
        cam = Camera.main.transform;
        timer = holdDuration;
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
    
    // 플레이어 충돌 시 삭제
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) Destroy(gameObject);
    }
}