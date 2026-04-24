#if UNITY_EDITOR
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LobbySceneSetup
{
    [MenuItem("Tools/Lobby/Setup Scene")]
    public static void Setup()
    {
        var scene = EditorSceneManager.GetActiveScene();

        var networkManager = EnsureNetworkManager();
        var lobbyManager   = EnsureLobbyManager();
        var canvas         = EnsureCanvas();
        EnsureEventSystem();

        var panel = BuildLobbyPanel(canvas.transform);
        var playerInput = CreateTMPInput(panel.transform,  "PlayerNameInput", "Player Name");
        var lobbyInput  = CreateTMPInput(panel.transform,  "LobbyNameInput",  "Lobby Name");
        var createBtn   = CreateTMPButton(panel.transform, "CreateButton",    "Create");
        var joinBtn     = CreateTMPButton(panel.transform, "JoinButton",      "Join");
        var statusText  = CreateTMPText(panel.transform,   "StatusText",      "");

        var lobbyUI = panel.AddComponent<LobbyUI>();
        lobbyUI.playerNameInput = playerInput;
        lobbyUI.lobbyNameInput  = lobbyInput;
        lobbyUI.createButton    = createBtn;
        lobbyUI.joinButton      = joinBtn;
        lobbyUI.statusText      = statusText;
        lobbyUI.lobbyPanel      = panel;

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = panel;

        Debug.Log("<b>[LobbySetup]</b> Scene configured. Save with Ctrl+S. " +
                  "Next: link project to Unity Cloud (Edit → Project Settings → Services) and " +
                  "launch Lobby + Relay at cloud.unity.com.");
    }

    static GameObject EnsureNetworkManager()
    {
        var existing = Object.FindFirstObjectByType<NetworkManager>();
        if (existing != null) return existing.gameObject;

        var go = new GameObject("NetworkManager");
        var nm = go.AddComponent<NetworkManager>();
        var transport = go.AddComponent<UnityTransport>();
        nm.NetworkConfig.NetworkTransport = transport;
        Undo.RegisterCreatedObjectUndo(go, "Create NetworkManager");
        return go;
    }

    static GameObject EnsureLobbyManager()
    {
        var existing = Object.FindFirstObjectByType<LobbyManager>();
        if (existing != null) return existing.gameObject;

        var go = new GameObject("LobbyManager");
        go.AddComponent<LobbyManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create LobbyManager");
        return go;
    }

    static Canvas EnsureCanvas()
    {
        var existing = Object.FindFirstObjectByType<Canvas>();
        if (existing != null) return existing;

        var go = new GameObject("Canvas");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
        return canvas;
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
    }

    static GameObject BuildLobbyPanel(Transform parent)
    {
        var go = new GameObject("LobbyPanel", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(500, 420);
        rt.anchoredPosition = Vector2.zero;

        var bg = go.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.65f);

        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.spacing = 15;
        vlg.childControlHeight    = true;
        vlg.childControlWidth     = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth  = true;

        Undo.RegisterCreatedObjectUndo(go, "Create LobbyPanel");
        return go;
    }

    static TMP_InputField CreateTMPInput(Transform parent, string name, string placeholder)
    {
        var resources = new TMP_DefaultControls.Resources();
        var go = TMP_DefaultControls.CreateInputField(resources);
        go.name = name;
        go.transform.SetParent(parent, false);

        var input = go.GetComponent<TMP_InputField>();
        if (input.placeholder is TMP_Text ph) ph.text = placeholder;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 48;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return input;
    }

    static Button CreateTMPButton(Transform parent, string name, string label)
    {
        var resources = new TMP_DefaultControls.Resources();
        var go = TMP_DefaultControls.CreateButton(resources);
        go.name = name;
        go.transform.SetParent(parent, false);

        var text = go.GetComponentInChildren<TMP_Text>();
        if (text != null) { text.text = label; text.alignment = TextAlignmentOptions.Center; }

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 48;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return go.GetComponent<Button>();
    }

    static TMP_Text CreateTMPText(Transform parent, string name, string content)
    {
        var resources = new TMP_DefaultControls.Resources();
        var go = TMP_DefaultControls.CreateText(resources);
        go.name = name;
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<TMP_Text>();
        text.text = content;
        text.alignment = TextAlignmentOptions.Center;

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 36;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return text;
    }
}
#endif
