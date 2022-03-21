using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Types = GraphQLCodeGen.Types;

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

    public class MyHandler : HttpClientHandler
    {
        public string AuthToken { get; }

        public MyHandler(string authToken)
        {
            AuthToken = authToken;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);

            return base.SendAsync(request, cancellationToken);
        }
    }

    private void InitSocketManager(string server)
    {
        var AuthToken = "abcd";
        // var httpMessageHandler = new MyHandler(AuthToken);
        // httpMessageHandler.Credentials = new NetworkCredential("test", "password");
        GraphQLHttpClientOptions options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(server),
            // HttpMessageHandler = httpMessageHandler,
            ConfigureWebsocketOptions =
                (options) =>
                {
                    options.SetRequestHeader("Authorization", "Bearer " + AuthToken);
                }
        };
        // HttpClient httpClient = new HttpClient(httpMessageHandler);
        // httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);

        var client =
            new GraphQLHttpClient(options, new NewtonsoftJsonSerializer());
        client.HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthToken}");
        client.WebsocketConnectionState.Subscribe(ConnectionStateChange);
        
            var request = new GraphQLRequest
            {
                Query = @"
    subscription {
        greetings {
value
slider
json
    }
    }"
            };

        UniRx.IObservable<GraphQLResponse<GreetingsResult>> subscriptionStream
            = client.CreateSubscriptionStream<GreetingsResult>(request);

        var greetingsObserver = new GreetingsObserver();
        greetingsObserver.OnSetValue += SetCubeValue;
        greetingsObserver.OnSetStringValue += LogItem;
        greetingsObserver.OnJSONMessage += OnJSONMessage;
        subscriptionStream.Subscribe(greetingsObserver);

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

    public class GreetingsObserver : UniRx.IObserver<GraphQLResponse<GreetingsResult>>
    {
        public Action<float> OnSetValue;
        public Action<string> OnSetStringValue;

        public void OnCompleted()
        {
            Debug.Log("Completed");
        }

        public void OnError(Exception error)
        {
            Debug.LogException(error);
        }

        public void OnNext(GraphQLResponse<GreetingsResult> value)
        {
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                // Debug.Log("Here");
                Debug.Log(value?.Data?.ToString());
                if (value?.Data?.Greetings.slider != 0)
                {
                    OnSetValue?.Invoke((float)value?.Data?.Greetings.slider);
                }
                else if (value?.Data?.Greetings.json != null)
                {
                    var jObject = JObject.Parse(value?.Data?.Greetings.json);

                    OnJSONMessage?.Invoke(jObject);
                }
                else if (value?.Data?.Greetings.value != null)
                {
                    OnSetStringValue?.Invoke(value?.Data?.Greetings.value);
                }
            });
        }

        public event Action<JObject> OnJSONMessage;
    }


    private void OnJSONMessage(JObject obj)
    {
        ExecuteOnMainThread.RunOnMainThread(() =>
        {
            foreach (var key in obj)
            {
                var i = Array.IndexOf(BarChartManager.BarLabel, key.Key);
                if (i >= 0)
                {
                    try
                    {
                        BarChartManager.SetBarSize(key.Key, Convert.ToSingle(key.Value));
                    }
                    catch (FormatException e)
                    {
                    }
                }
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

public class GreetingsResult
{
    public Types.Result Greetings;

    public override string ToString()
    {
        return JsonConvert.SerializeObject(Greetings);
    }
}
