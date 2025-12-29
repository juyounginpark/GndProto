using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    public float explodeScale = 2f;   // 얼마나 커질지
    public float explodeTime = 0.15f; // 커지는 데 걸리는 시간

    private bool isDead = false;
    private SpriteRenderer sr;
    private EnemyHealth health;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<EnemyHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDead && other.CompareTag("Player"))
        {
            // 플레이어에게 닿으면 1 데미지
            health.TakeDamage(1);
        }
    }

    public void OnDeath()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(ExplodeCoroutine());
    }

    IEnumerator ExplodeCoroutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * explodeScale;

        float t = 0f;

        while (t < explodeTime)
        {
            t += Time.deltaTime;
            float lerp = t / explodeTime;

            // 1. 스케일 점점 키우기
            transform.localScale = Vector3.Lerp(startScale, endScale, lerp);

            // 2. 알파값 줄여서 사라지는 느낌
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f - lerp;
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
