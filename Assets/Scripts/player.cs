using UnityEngine;
using UnityEngine.InputSystem;   // New Input System

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 15f;

    private Rigidbody2D rb;
    private float horizontalInput = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 키보드 입력 (←, →, A, D)
        if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;
        else
            horizontalInput = 0f;
    }

    void FixedUpdate()
    {
        // 물리 기반 이동 → Collider 충돌 제대로 작동
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }
}
