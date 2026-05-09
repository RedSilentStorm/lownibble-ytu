using UnityEngine;
using UnityEngine.InputSystem;

public class ComicInputHandler : MonoBehaviour
{
    private GameInput input;

    private void Awake()
    {
        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Comic.Enable();
        // Parry, dodge gibi eylemleri dinleyeceğiz ama şimdilik sadece geçiş için:
        input.Comic.Advance.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        input.Comic.Disable();
        input.Comic.Advance.performed -= OnAttackPerformed;
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        GameManager.Instance.SwitchToFight();
    }
}