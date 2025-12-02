using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float moveSpeed = 8f;        // 총알 속도
    public float homingDuration = 1.5f; // N초: 유도하는 시간
    public float lifeTime = 5f;         // 총알 전체 수명

    private Transform player;
    private Rigidbody2D rb;
    private float timer;
    private bool isHoming = true;
    private Vector2 fixedDirection;     // 유도 끝난 후 유지할 방향

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Player 찾기 (Tag가 "Player"여야 함)
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        timer = homingDuration;

        // 혹시 플레이어가 없을 수도 있으니, 없으면 앞으로 직진하게 기본 방향 설정
        if (player != null)
        {
            fixedDirection = ((Vector2)player.position - rb.position).normalized;
        }
        else
        {
            fixedDirection = transform.right; // 그냥 오른쪽 기준
        }

        // 최초 속도 설정
        rb.linearVelocity = fixedDirection * moveSpeed;

        // N초 + 여유로 lifeTime 이후 파괴
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!isHoming)
            return;

        timer -= Time.deltaTime;

        if (player != null)
        {
            // 매 프레임 플레이어 방향을 다시 계산
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;

            // 바로 꺾고 싶으면 그냥 dir 사용
            fixedDirection = dir;
            rb.linearVelocity = fixedDirection * moveSpeed;

            // 부드럽게 꺾고 싶으면 아래처럼 Lerp나 Slerp로 보간 가능
            // Vector2 newDir = Vector2.Lerp(rb.velocity.normalized, dir, 0.1f).normalized;
            // fixedDirection = newDir;
            // rb.velocity = newDir * moveSpeed;
        }

        // N초 지난 시점부터는 직선 모드
        if (timer <= 0f)
        {
            isHoming = false;
            rb.linearVelocity = fixedDirection * moveSpeed; // 그 순간의 방향 유지
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: Player 데미지 처리
            // other.GetComponent<PlayerHealth>()?.TakeDamage(1);

            Destroy(gameObject);
        }
        else if (other.CompareTag("WALL"))
        {
            Destroy(gameObject);
        }
    }
}
