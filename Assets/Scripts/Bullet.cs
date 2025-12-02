using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f; 

    private Rigidbody2D rb;
    private bool isClone = false; // 클론 여부 확인

    void Start()
    {
        // ★ 핵심: 이름 뒤에 "(Clone)"이 붙어있는지 확인
        // 복제된 애들은 자동으로 이름 뒤에 (Clone)이 붙습니다.
        if (gameObject.name.Contains("(Clone)"))
        {
            isClone = true;
        }
        else
        {
            // 원본이라면 움직이지도, 삭제 로직을 돌지도 않게 여기서 종료
            return; 
        }

        rb = GetComponent<Rigidbody2D>();
        
        // 1. 플레이어 찾기
        GameObject target = GameObject.FindGameObjectWithTag("Player");
        Vector2 dir;

        if (target != null)
        {
            // 2. 방향 계산
            dir = (target.transform.position - transform.position).normalized;
        }
        else
        {
            dir = Vector2.down;
        }

        // 3. 회전
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 4. 속도 적용
        // Unity 6.0(2023) 이상: rb.linearVelocity / 구버전: rb.velocity
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = dir * speed;
#else
        rb.velocity = dir * speed;
#endif
    }

    void Update()
    {
        // ★ 원본이면 업데이트 로직(삭제 체크)도 실행하지 않음
        if (!isClone) return;

        // 5. 화면 밖으로 나갔는지 체크하여 삭제
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
        return viewPos.x < -0.1f || viewPos.x > 1.1f || viewPos.y < -0.1f || viewPos.y > 1.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ★ 원본은 충돌 처리도 안 함 (안전장치)
        if (!isClone) return;

        if (other.CompareTag("Player") || other.CompareTag("WALL"))
        {
            Destroy(gameObject);
        }
    }
}