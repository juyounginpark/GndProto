using UnityEngine;

public class StraightBullet : MonoBehaviour
{
    [Header("기본 설정")]
    public float speed = 15f;          // 총알 날아가는 속도

    [Header("가드 시 튕김 설정")]
    public float bounceForce = 20f;    // 튕겨나가는 힘의 세기

    private Rigidbody2D rb;
    private bool isDeflected = false;  // 튕겨나갔는지 확인하는 플래그
    private bool isClone = false;      // 복제된 녀석인지 확인하는 플래그

    void Start()
    {
        // 1. 클론 체크 (프리팹 원본은 로직 실행 X)
        if (gameObject.name.Contains("(Clone)"))
        {
            isClone = true;
        }
        else
        {
            this.enabled = false; // 원본이면 스크립트 끔
            return;
        }

        rb = GetComponent<Rigidbody2D>();

        // 2. 배경보다 앞에 보이게 Z축 고정 (-5)
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);

        // 3. 플레이어 방향 계산
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        Vector2 dir;

        if (target != null)
        {
            // 플레이어 쪽으로 방향 잡기
            dir = (target.transform.position - transform.position).normalized;
        }
        else
        {
            // 플레이어 없으면 그냥 아래로
            dir = Vector2.down;
        }

        // 4. 총알 머리 회전 (진행 방향 보기)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 5. 속도 적용 (물리 엔진 사용)
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = dir * speed;
#else
        rb.velocity = dir * speed;
#endif
    }

    void Update()
    {
        // 원본이거나 이미 튕겨나갔으면 업데이트 안 함
        if (!isClone || isDeflected) return;

        // 6. 화면 밖으로 나갔는지 체크하여 삭제
        if (IsOutOfBounds())
        {
            Destroy(gameObject);
        }
    }

    // 화면 밖 판별 함수
    bool IsOutOfBounds()
    {
        if (Camera.main == null) return false;
        
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        // 화면 범위를 넉넉하게 -0.2 ~ 1.2로 설정
        return viewPos.x < -0.2f || viewPos.x > 1.2f || viewPos.y < -0.2f || viewPos.y > 1.2f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 원본이거나 이미 튕겨나갔으면 무시
        if (!isClone || isDeflected) return;

        if (other.CompareTag("Player"))
        {
            // 플레이어의 가드 스크립트 가져오기
            PlayerGuard guard = other.GetComponent<PlayerGuard>();

            // 가드 성공 여부 확인
            if (guard != null && guard.IsGuarding)
            {
                Debug.Log("🛡️ 총알 역방향 반사!");
                isDeflected = true; // 이제부터 일반 이동 로직 정지

                // --- 역방향 튕겨내기 로직 ---
                
                // 1. 현재 날아오던 속도 벡터 가져오기
#if UNITY_6000_0_OR_NEWER
                Vector2 incomingVelocity = rb.linearVelocity;
#else
                Vector2 incomingVelocity = rb.velocity;
#endif
                // 혹시 속도가 0일 경우 대비 (플레이어 반대 방향으로 설정)
                if (incomingVelocity == Vector2.zero)
                {
                    incomingVelocity = (other.transform.position - transform.position).normalized;
                }

                // 2. 속도 초기화 (물리력 충돌 방지)
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector2.zero;
#else
                rb.velocity = Vector2.zero;
#endif

                // 3. 반대 방향 계산 (들어오던 방향의 -1배)
                Vector2 reflectDir = -incomingVelocity.normalized; 

                // 4. 힘 가하기 (Impulse: 순간적인 힘)
                rb.AddForce(reflectDir * bounceForce, ForceMode2D.Impulse);

                // 5. 뱅글뱅글 회전 효과 (타격감)
                rb.angularVelocity = 720f; 

                // 6. 더 이상 플레이어와 충돌하지 않게 콜라이더 끄기
                Collider2D col = GetComponent<Collider2D>();
                if(col != null) col.enabled = false;

                // 7. 2초 뒤에 완전히 삭제
                Destroy(gameObject, 2f);
            }
            else
            {
                // 가드 실패 -> 플레이어 피격 -> 총알 삭제
                // TODO: PlayerHealth.TakeDamage(1); 
                Destroy(gameObject); 
            }
        }
        else if (other.CompareTag("WALL"))
        {
            // 벽에 닿으면 삭제
            Destroy(gameObject);
        }
    }
}