using UnityEngine;
using UnityEngine.InputSystem;   // New Input System

public class CameraFollow : MonoBehaviour
{
    public float normalSpeed = 40f;
    public float glideSpeed = 15f;

    private float currentSpeed;

    void Update()
    {
        // 스페이스를 누르고 있는 동안만 boost
        if (Keyboard.current.spaceKey.isPressed)
        {
            currentSpeed = glideSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }

        // 카메라 이동
        transform.position += Vector3.down * currentSpeed * Time.deltaTime;
    }
}
