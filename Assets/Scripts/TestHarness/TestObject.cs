using System;
using System.Collections.Generic;
using System.Data.Common;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TestObject : MonoBehaviour
{
    GraphQLRequest graphQlRequest;
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
            // if (graphQlRequest != null)
            // {
            //     graphQlRequest.Disconnect();
            // }

            InitSocketManager(server);
        };
    }

    private void InitSocketManager(string server)
    {
        var client = new GraphQLHttpClient(server, new NewtonsoftJsonSerializer()); // todo importing the dll for the newtonsoft shit is fucking up somehow... idk if it matters? but it's ugly
        client.WebsocketConnectionState.Subscribe(ConnectionStateChange);
        
        var request = new GraphQLRequest
        {
            Query = @"
    subscription {
        greetings
    }"
        };
        
        IObservable<GraphQLResponse<UserJoinedSubscriptionResult>> subscriptionStream 
            = client.CreateSubscriptionStream<UserJoinedSubscriptionResult>(request);

        subscriptionStream.Subscribe(new TemperatureReporter());

        // graphQlRequest.OnSendMessage += LogItem;
        // graphQlRequest.OnSetValue += SetCubeValue;
        // graphQlRequest.OnJSONMessage += OnJSONMessage;
        // graphQlRequest.OnConnected += () =>
        // {
        //     ExecuteOnMainThread.RunOnMainThread(() =>
        //     {
        //         ConnectionStatus.text = "Connected";
        //         ConnectionStatus.color = Color.green;
        //     });
        // };
        // graphQlRequest.OnDisconnected += () =>
        // {
        //     ExecuteOnMainThread.RunOnMainThread(() =>
        //     {
        //         ConnectionStatus.text = "Disconnected";
        //         ConnectionStatus.color = Color.red;
        //     });
        // };
        // graphQlRequest.OnReconnecting += () =>
        // {
        //     ExecuteOnMainThread.RunOnMainThread(() =>
        //     {
        //         ConnectionStatus.text = "Reconnecting...";
        //         ConnectionStatus.color = Color.yellow;
        //     });
        //     
        // };
    }

    private void ConnectionStateChange(GraphQLWebsocketConnectionState obj)
    {
        ExecuteOnMainThread.RunOnMainThread(() =>
        {
            switch (obj)
            {
                case GraphQLWebsocketConnectionState.Connected:
                    ConnectionStatus.text = nameof(GraphQLWebsocketConnectionState.Connected);
                    ConnectionStatus.color = Color.green;
                    break;
                case GraphQLWebsocketConnectionState.Disconnected:
                    ConnectionStatus.text = nameof(GraphQLWebsocketConnectionState.Disconnected);
                    ConnectionStatus.color = Color.red;
                    break;
                case GraphQLWebsocketConnectionState.Connecting:
                    ConnectionStatus.text = nameof(GraphQLWebsocketConnectionState.Connecting);
                    ConnectionStatus.color = Color.yellow;
                    break;

            }
        });

    }

    public class TemperatureReporter : IObserver<GraphQLResponse<UserJoinedSubscriptionResult>>
    {
        public void OnCompleted()
        {
            Debug.Log("Completed");
        }

        public void OnError(Exception error)
        {
            Debug.LogException(error);
        }

        public void OnNext(GraphQLResponse<UserJoinedSubscriptionResult> value)
        {
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                Debug.Log("Here");
                Debug.Log(value?.Data?.Greetings);
            });
        }
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
        // graphQlRequest.Disconnect();

        ClearItems();
    }
}

public class UserJoinedSubscriptionResult
{
    public string Greetings;
}