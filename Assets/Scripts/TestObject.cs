using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Socket.Quobject.EngineIoClientDotNet.Modules;
using Socket.Quobject.EngineIoClientDotNet.Thread;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class TestObject : MonoBehaviour
{
    private QSocket socket;

    public GameObject TextPrefab;
    public Transform ListViewArea;
    public GameObject Cube;
    SynchronizationContext syncContext;

    private void Awake()
    {
        // On main thread, during initialization:
        syncContext = System.Threading.SynchronizationContext.Current;

        var exists = GameObject.Find(nameof(ExecuteOnMainThread));
        if (exists == null)
        {
            new GameObject(nameof(ExecuteOnMainThread)).AddComponent<ExecuteOnMainThread>();
        }

#if  UNITY_EDITOR
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
        LogManager.Enabled = true;

        Debug.Log("start");
        socket = IO.Socket("http://192.168.86.238:11002/", new IO.Options()
        {
            Query = new Dictionary<string, string>
            {
                {"token", "V2"}
            },
            // Path = "/socket.io/nsp"
        });

        socket.On(QSocket.EVENT_CONNECT, () =>
        {
            Debug.Log("Connected");
            LogItem("Connected");
            socket.Emit("chat", "test");
            socket.Emit("welcome", "test");
            socket.Emit("hi", DateTime.Now.ToString());
        });

        socket.On("chat", data => { Debug.Log("data : " + data); });

        socket.On("web message", data =>
        {
            Debug.Log("web message : " + data);

            LogItem(data as string);
        });
        
        socket.On("set value", data =>
        {
            Debug.Log("set value " + data);
            ExecuteOnMainThread.RunOnMainThread(() =>
            {
                var transformLocalScale = Cube.transform.localScale;
                float result = 1;
                if (float.TryParse(data as string, out result))
                {
                    transformLocalScale.y = result;
                    Cube.transform.localScale = transformLocalScale;
                }
            });
        });
        
        

        socket.On(QSocket.EVENT_DISCONNECT, () =>
        {
            Debug.Log("EVENT_DISCONNECT");
            LogItem("Disconnected");
        });

        socket.On(QSocket.EVENT_CONNECT_ERROR, () =>
        {
            Debug.Log("EVENT_CONNECT_ERROR");
            LogItem("EVENT_CONNECT_ERROR");
        });

        socket.On(QSocket.EVENT_RECONNECTING, () =>
        {
            Debug.Log("EVENT_RECONNECTING");
            LogItem("Reconnecting");
        });

        socket.On(QSocket.EVENT_CONNECT_TIMEOUT, () =>
        {
            Debug.Log("EVENT_CONNECT_TIMEOUT");
            LogItem("ConnectTimeout");
        });

        socket.On(QSocket.EVENT_ERROR, (err) => { Debug.LogException(err as Exception); });

        socket.On("hi", response =>
        {
            // Console.WriteLine(response.ToString());
            Debug.Log(response);
        });

        socket.On("welcome", response =>
        {
            // Console.WriteLine(response.ToString());
            Debug.Log(System.Text.Encoding.UTF8.GetString(response as byte[] ?? Array.Empty<byte>()));
        });


        // socket.Connect();

        ClearItems();
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
        socket.Disconnect();
        
        ClearItems();
    }

}