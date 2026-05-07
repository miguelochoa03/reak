using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LeaderboardDisplay : MonoBehaviour
{
    public TMP_Text scoreText;
    [Tooltip("Header text shown above the entries.")]
    public string header = "LEADERBOARD";
    [Tooltip("How many entries to show.")]
    public int maxEntries = 8;
    [Tooltip("How often (seconds) the display refreshes.")]
    public float refreshInterval = 0.5f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < refreshInterval) return;
        timer = 0f;
        Refresh();
    }

    void Refresh()
    {
        if (scoreText == null) return;
        if (NetworkManager.Singleton == null)
        {
            scoreText.text = $"<b>{header}</b>\n(no NetworkManager)";
            return;
        }
        if (!NetworkManager.Singleton.IsListening)
        {
            scoreText.text = $"<b>{header}</b>\n(not listening)";
            return;
        }

        var entries = new List<(string name, int score)>();

        // Server has access to all connected clients; remote clients only see themselves via ConnectedClientsList.
        // Use SpawnManager.SpawnedObjectsList as a fallback so all clients can see all PlayerScores.
        var seen = new HashSet<ulong>();
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var c in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (c.PlayerObject == null) continue;
                var ps = c.PlayerObject.GetComponent<PlayerScore>();
                if (ps == null) continue;
                entries.Add((ps.PlayerName.Value.ToString(), ps.Score.Value));
                seen.Add(c.ClientId);
            }
        }
        foreach (var kv in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
        {
            var ps = kv.GetComponent<PlayerScore>();
            if (ps == null) continue;
            if (seen.Contains(kv.OwnerClientId)) continue;
            entries.Add((ps.PlayerName.Value.ToString(), ps.Score.Value));
        }

        entries.Sort((a, b) => b.score.CompareTo(a.score));

        var sb = new StringBuilder();
        sb.AppendLine($"<b>{header}</b>");
        for (int i = 0; i < entries.Count && i < maxEntries; i++)
            sb.AppendLine($"{i + 1}. {entries[i].name} — {entries[i].score}");

        scoreText.text = sb.ToString();
    }
}
