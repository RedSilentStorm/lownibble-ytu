using UnityEngine;
using System.Collections.Generic;

public class QTEInputHandler : MonoBehaviour
{
    // İzlenecek tüm QTE tuşlarının birleşik havuzu
    private readonly KeyCode[] allQTETypes = {
        KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D,
        KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.U, KeyCode.I, KeyCode.O,
        KeyCode.C
    };

    void Update()
    {
        if (ReactionWindowManager.Instance == null || !ReactionWindowManager.Instance.IsWindowOpen) return;

        foreach (KeyCode k in allQTETypes)
        {
            if (Input.GetKeyDown(k))
            {
                // Kopya liste üzerinde dön
                List<QTEDisplay> displays = ReactionUI.Instance.GetActiveDisplays();
                foreach (var d in displays)
                {
                    if (d != null) d.OnKeyPressed(k);
                }
            }
        }
    }
}