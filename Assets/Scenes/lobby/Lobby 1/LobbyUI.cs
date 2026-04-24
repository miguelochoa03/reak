using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField playerNameInput;
    public TMP_InputField lobbyNameInput;

    [Header("Buttons")]
    public Button createButton;
    public Button joinButton;

    [Header("Optional")]
    public TMP_Text statusText;
    public GameObject lobbyPanel;

    void Start()
    {
        createButton.onClick.AddListener(OnCreateClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    async void OnCreateClicked()
    {
        if (!Validate()) return;
        SetButtons(false);
        SetStatus("Creating lobby...");
        bool ok = await LobbyManager.Instance.CreateLobby(lobbyNameInput.text, playerNameInput.text);
        SetStatus(ok ? "Hosting!" : "Create failed — see Console");
        if (ok && lobbyPanel) lobbyPanel.SetActive(false);
        else SetButtons(true);
    }

    async void OnJoinClicked()
    {
        if (!Validate()) return;
        SetButtons(false);
        SetStatus("Joining lobby...");
        bool ok = await LobbyManager.Instance.JoinLobbyByName(lobbyNameInput.text, playerNameInput.text);
        SetStatus(ok ? "Joined!" : "Join failed — see Console");
        if (ok && lobbyPanel) lobbyPanel.SetActive(false);
        else SetButtons(true);
    }

    bool Validate()
    {
        if (string.IsNullOrWhiteSpace(playerNameInput.text)) { SetStatus("Enter a player name."); return false; }
        if (string.IsNullOrWhiteSpace(lobbyNameInput.text))  { SetStatus("Enter a lobby name.");  return false; }
        return true;
    }

    void SetButtons(bool interactable)
    {
        createButton.interactable = interactable;
        joinButton.interactable = interactable;
    }

    void SetStatus(string msg)
    {
        Debug.Log($"[LobbyUI] {msg}");
        if (statusText) statusText.text = msg;
    }
}
