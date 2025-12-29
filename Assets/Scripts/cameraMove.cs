using UnityEngine;
using UnityEngine.InputSystem;   // New Input System

public class CameraFollow : MonoBehaviour
{
    public float normalSpeed = 40f;
    public float glideSpeed = 15f;

    // 맵 스크롤용 (카메라 y좌표 변경 없이 낙하 효과)
    public static float ScrollOffset { get; private set; } = 0f;
    public static float CurrentSpeed { get; private set; } = 40f;

    void Start()
    {
        CurrentSpeed = normalSpeed;
    }

    void Update()
    {
        // 스페이스를 누르고 있는 동안만 glide (느리게)
        if (Keyboard.current.spaceKey.isPressed)
        {
            CurrentSpeed = glideSpeed;
        }
        else
        {
            CurrentSpeed = normalSpeed;
        }

        // 카메라 y좌표는 고정, 스크롤 오프셋만 누적
        ScrollOffset -= CurrentSpeed * Time.deltaTime;
    }

    public static void ResetScroll()
    {
        ScrollOffset = 0f;
    }
}
