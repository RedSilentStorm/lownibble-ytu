using UnityEngine;
using UnityEngine.InputSystem;

public class BattleInputHandler : MonoBehaviour
{
    private GameInput input;

    private void Awake()
    {
        input = new GameInput();
    }

    private void OnEnable()
    {
        input.Fight.Enable();
        input.Fight.Parry.performed += OnParry;
        input.Fight.Dodge.performed += OnDodge;
        // Attack zaten FightUI'den butonla, burada gerek yok.
    }

    private void OnDisable()
    {
        input.Fight.Disable();
        input.Fight.Parry.performed -= OnParry;
        input.Fight.Dodge.performed -= OnDodge;
    }

    private void OnParry(InputAction.CallbackContext ctx)
    {
        if (ReactionWindowManager.Instance != null)
            ReactionWindowManager.Instance.ParryPressed();
    }

    private void OnDodge(InputAction.CallbackContext ctx)
    {
        if (ReactionWindowManager.Instance != null)
            ReactionWindowManager.Instance.DodgePressed();
    }
}