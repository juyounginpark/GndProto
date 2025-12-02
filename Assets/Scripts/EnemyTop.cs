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
    private enum State { Idle, Rising, Holding, Leaving } // Idle 상태 추가
    private State state = State.Idle; // Idle에서 시작
    private float timer;
    private bool hasFired = false;
    private bool isActivated = false; // Update 중복 진입 방지 플래그

    void Start()
    {
        cam = Camera.main.transform;
        
        // 1. 프리팹 연결 체크
        if (bulletPrefab == null)
        {
            Debug.LogError("!!! 총알 프리팹이 연결되지 않았습니다 !!! Inspector를 확인하세요.");
        }
    }

    void Update()
    {
        if (cam == null) return;

        // 아직 활성화되지 않았다면, 카메라 뷰에 들어왔는지 계속 체크
        if (!isActivated)
        {
            float camBottomY = cam.position.y - Camera.main.orthographicSize;
            if (transform.position.y > camBottomY)
            {
                isActivated = true; // 활성화!
                state = State.Rising;
            }
            else
            {
                return; // 아직 활성화될 때가 아니면 아무것도 안 함
            }
        }

        // --- 활성화된 후의 로직 ---
        
        float camTopY = cam.position.y + Camera.main.orthographicSize;

        switch (state)
        {
            case State.Rising:
                float targetY = camTopY - offsetFromTop;
                Vector3 pos = transform.position;
                pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
                transform.position = pos;

                float distance = Mathf.Abs(pos.y - targetY);
                if (distance < 1.0f) 
                {
                    state = State.Holding;
                    timer = attackDelay;
                }
                break;

            case State.Holding:
                targetY = camTopY - offsetFromTop;
                transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

                timer -= Time.deltaTime;

                if (timer <= 0f && !hasFired)
                {
                    FireBullet();
                    hasFired = true;
                    timer = afterAttackDelay;
                }

                if (hasFired && timer <= 0f)
                {
                    state = State.Leaving;
                }
                break;

            case State.Leaving:
                transform.Translate(Vector3.up * leaveSpeed * Time.deltaTime);

                if (transform.position.y > camTopY + destroyOffset)
                {
                    Destroy(gameObject);
                }
                break;
        }
    }

    void FireBullet()
    {
        // 2. 발사 시점에 로그 출력
        Debug.Log("총알 발사 함수 호출됨!"); 

        if (bulletPrefab != null)
        {
            Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            Debug.Log(">>> 총알 생성 성공!");
        }
        else
        {
            Debug.LogError("!!! 총알을 생성하려 했으나 프리팹이 없습니다 !!!");
        }
    }
}