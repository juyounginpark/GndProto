
using UnityEngine;

public class TopEnemy : MonoBehaviour
{
    [Header("이동 관련")]
    public float riseSpeed = 5f;          // 위로 올라가는 속도
    public float offsetFromTop = 1f;      // 카메라 상단에서 얼마나 아래에 위치할지

    [Header("대기 및 공격")]
    public float holdDuration = 3f;       // 상단에 머무르는 시간 N초
    public float fireInterval = 0.5f;     // 총알 발사 간격

    [Header("총알")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 8f;
    public float bulletLifeTime = 5f;

    private Transform cam;
    private Transform player;

    private enum State { Rising, Holding, Done }
    private State state = State.Rising;

    private float holdTimer;
    private float fireTimer;

    void Start()
    {
        cam = Camera.main.transform;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        holdTimer = holdDuration;
        fireTimer = 0f;
    }

    void Update()
    {
        if (cam == null) return;

        switch (state)
        {
            case State.Rising:
                UpdateRising();
                break;
            case State.Holding:
                UpdateHolding();
                break;
        }
    }

    void UpdateRising()
    {
        Camera cameraComp = Camera.main;
        float camHalfHeight = cameraComp.orthographicSize;

        // 카메라 상단 기준 목표 y
        float targetY = cam.position.y + camHalfHeight - offsetFromTop;

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, riseSpeed * Time.deltaTime);
        transform.position = pos;

        // 목표 위치에 거의 도착하면 상태 전환
        if (Mathf.Abs(pos.y - targetY) < 0.01f)
        {
            state = State.Holding;
        }
    }

    void UpdateHolding()
    {
        Camera cameraComp = Camera.main;
        float camHalfHeight = cameraComp.orthographicSize;

        // 카메라 상단에 자연스럽게 붙어 있도록, 매 프레임 y를 카메라 기준으로 재설정
        float topY = cam.position.y + camHalfHeight - offsetFromTop;
        transform.position = new Vector3(transform.position.x, topY, transform.position.z);

        // 시간 카운트
        holdTimer -= Time.deltaTime;
        fireTimer -= Time.deltaTime;

        // 총알 발사
        if (fireTimer <= 0f)
        {
            FireBullet();
            fireTimer = fireInterval;
        }

        // N초 지나면 사라짐
        if (holdTimer <= 0f)
        {
            state = State.Done;
            Destroy(gameObject);
        }
    }

    void FireBullet()
    {
        if (bulletPrefab == null)
            return;

        // 방향/속도는 HomingBullet이 알아서 처리하므로 위치만 넘겨주면 됨
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }
}
