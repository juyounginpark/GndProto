using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerGuard : MonoBehaviour
{
    [Header("방패 오브젝트 연결")]
    public GameObject shieldObject; // GuardShield 연결

    public bool IsGuarding { get; private set; } = false;

    void Update()
    {
        // 키보드가 연결되어 있는지 확인(null 체크) 후 상태 확인
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
        {
            IsGuarding = true;
            if (shieldObject != null) shieldObject.SetActive(true);
        }
        else
        {
            IsGuarding = false;
            if (shieldObject != null) shieldObject.SetActive(false);
        }
    }
}