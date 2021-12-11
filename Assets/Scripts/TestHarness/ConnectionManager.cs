using System;
using UnityEngine;

internal class ConnectionManager
{
    private const string kLastLocalPeerId = "lastLocalPeerId";
    
    public string Server => PlayerPrefs.GetString(kLastLocalPeerId, "");
    public event Action<string> OnServerChange;

    public void SetServer(string serverText)
    {
        PlayerPrefs.SetString(kLastLocalPeerId, serverText);

        OnServerChange?.Invoke(serverText);
    }

    public bool HasServer()
    {
        return ! string.IsNullOrEmpty(Server);
    }

}