using System;
using System.Collections.Generic;
using System.Linq;
using Scope.RemoteAR.SocketIO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TestObject : MonoBehaviour
{
    SocketIOManager socketIOManager;
    private ConnectionManager _connectionManager;

    public GameObject TextPrefab;
    public Transform ListViewArea;
    public GameObject Cube;
    public InputField Server;
    public Text ConnectionStatus;
    public BarChartManager BarChartManager;

    private void Awake()
    {
        // On main thread, during initialization:

        var exists = GameObject.Find(nameof(ExecuteOnMainThread));
        if (exists == null)
        {
            new GameObject(nameof(ExecuteOnMainThread)).AddComponent<ExecuteOnMainThread>();
        }

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += HandleOnPlayModeChanged;
#endif

        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    }

#if UNITY_EDITOR

    private void HandleOnPlayModeChanged(PlayModeStateChange obj)
    {
        if (obj == PlayModeStateChange.ExitingPlayMode)
        {
            ClearItems();
        }
    }
#endif

    private void ClearItems()
    {
        foreach (Transform child in ListViewArea.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        int childs = ListViewArea.transform.childCount;
        for (int i = childs - 1; i > 0; i--)
        {
            GameObject.Destroy(ListViewArea.transform.GetChild(i).gameObject);
        }
    }

    void Start()
    {
        Debug.Log("start");

        InitSocketManager();
        InitUI();

        ClearItems();
    }

    private void InitSocketManager()
    {
        _connectionManager = new ConnectionManager();

        if (_connectionManager.HasServer())
        {
            InitSocketManager(_connectionManager.Server);
        }

        _connectionManager.OnServerChange += server =>
        {
            if (socketIOManager != null)
            {
                socketIOManager.Disconnect();
            }

            InitSocketManager(server);
        };
    }

    private void InitSocketManager(string server)
    {
        socketIOManager = new SocketIOManager(server);
        socketIOManager.OnSendMessage += LogItem;
        socketIOManager.OnSetValue += SetCubeValue;
        socketIOManager.OnJSONMessage += OnJSONMessage;
        socketIOManager.OnConnected += () =>
        {
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                ConnectionStatus.text = "Connected";
                ConnectionStatus.color = Color.green;
            });
        };
        socketIOManager.OnDisconnected += () =>
        {
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                ConnectionStatus.text = "Disconnected";
                ConnectionStatus.color = Color.red;
            });
        };
        socketIOManager.OnReconnecting += () =>
        {
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                ConnectionStatus.text = "Reconnecting...";
                ConnectionStatus.color = Color.yellow;
            });
            
        };
    }

    private void OnJSONMessage(Dictionary<string, string> obj)
    {
        ExecuteOnMainThread.RunOnMainThread(() =>
        {
            foreach (var key in obj)
            {
                var i = Array.IndexOf(BarChartManager.BarLabel, key.Key);
                BarChartManager.SetBarSize(key.Key, Convert.ToSingle(key.Value));
            }
        });
    }

    private void InitUI()
    {
        if (_connectionManager.HasServer())
        {
            Server.text = _connectionManager.Server;
        }
        
        Server.onEndEdit.AddListener(delegate
        {
            Uri uriResult;
            var serverText = Server.text;
            if (!Uri.TryCreate(serverText, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                serverText = "http://" + serverText;
            }
            
            if (Uri.TryCreate(serverText, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                _connectionManager.SetServer(uriResult.AbsoluteUri);
            }
        });
    }

    private void SetCubeValue(float value)
    {
        ExecuteOnMainThread.RunOnMainThread(() =>
        {
            var transformLocalScale = Cube.transform.localScale;

            transformLocalScale.y = value;
            Cube.transform.localScale = transformLocalScale;
        });
    }

    private void LogItem(string data)
    {
// On your worker thread
        ExecuteOnMainThread.RunOnMainThread(() =>
        {
            // This code here will run on the main thread
            GameObject o = Instantiate(TextPrefab, ListViewArea, false);
            o.GetComponent<Text>().text = data;
            o.SetActive(true);

            RectTransform rt = ListViewArea.GetComponent<RectTransform>();
            var rtSizeDelta = rt.sizeDelta;
            rtSizeDelta.y = 30 * ListViewArea.childCount;
            rt.sizeDelta = rtSizeDelta;

            FindObjectOfType<ScrollRect>().normalizedPosition = new Vector2(0, 0);
        });
    }

    private void OnDestroy()
    {
        socketIOManager.Disconnect();

        ClearItems();
    }
}