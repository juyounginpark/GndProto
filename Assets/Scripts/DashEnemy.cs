using UnityEngine;

public class DashEnemy : MonoBehaviour
{
    [Header("이동 설정")]
    public float riseSpeed = 5f;        // 등장 속도
    public float offsetFromTop = 1.5f;  // 카메라 상단에서 멈출 위치 (고드름 매달리는 위치)
    
    [Header("공격 설정")]
    public float holdDuration = 1.0f;   // 매달려서 대기하는 시간
    public float dashSpeed = 30f;       // 낙하 속도 (아주 빠르게)
    public float destroyOffset = 3f;    // 화면 아래로 얼마나 벗어나면 삭제할지

    private Transform cam;
    private enum State { Rising, Holding, Dashing }
    private State state = State.Rising;

    private float timer;

    void Start()
    {
        cam = Camera.main.transform;
        timer = holdDuration;
    }

    void Update()
    {
        if (cam == null) return;

        // 카메라 상단, 하단 좌표 계산 (매 프레임 갱신)
        float camHalfHeight = Camera.main.orthographicSize;
        float camTopY = cam.position.y + camHalfHeight;
        float camBottomY = cam.position.y - camHalfHeight;
        
        // 목표 고정 높이
        float targetY = camTopY - offsetFromTop;

        switch (state)
        {
            case State.Rising:
                // 1. 등장: X는 그대로 두고 Y만 목표지점으로 이동
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
                transform.position = pos;

                // 목표 높이 도착 시 Holding 전환
                if (Mathf.Abs(pos.y - targetY) < 0.1f)
                {
                    state = State.Holding;
                }
                break;

            case State.Holding:
                // 2. 고정 (고드름): 
                // X축: 절대 움직이지 않음 (처음 생성된 위치 유지)
                // Y축: 카메라가 움직이면 같이 움직여야 하므로 카메라 기준 위치로 고정
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

                // 대기 시간 카운트
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    state = State.Dashing;
                }
                break;

            case State.Dashing:
                // 3. 낙하: 카메라 무시하고 월드 좌표 기준 수직 하강
                transform.Translate(Vector3.down * dashSpeed * Time.deltaTime);

                // 화면 아래로 완전히 사라지면 삭제
                if (transform.position.y < camBottomY - destroyOffset)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    // 플레이어 충돌 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: 플레이어 데미지 로직
            // other.GetComponent<PlayerHealth>()?.TakeDamage(1);
            
            // 충돌 후 삭제 (선택 사항: 관통하고 싶으면 이 줄 삭제)
            Destroy(gameObject); 
        }
        else if (other.CompareTag("WALL")) // 벽에 닿으면 삭제하고 싶을 경우
        {
            Destroy(gameObject);
        }
    }
}