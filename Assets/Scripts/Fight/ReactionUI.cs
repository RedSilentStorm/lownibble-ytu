using UnityEngine;

public class ReactionUI : MonoBehaviour
{
    public static ReactionUI Instance { get; private set; }

    [SerializeField] private GameObject reactionPanel;

    private void Awake()
    {
        Instance = this;
        reactionPanel.SetActive(false);
    }

    public void ShowWindow()
    {
        reactionPanel.SetActive(true);
    }

    public void HideWindow()
    {
        reactionPanel.SetActive(false);
    }
}