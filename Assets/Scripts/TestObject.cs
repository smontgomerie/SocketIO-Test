using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Socket.Quobject.EngineIoClientDotNet.Modules;
using Socket.Quobject.EngineIoClientDotNet.Thread;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class TestObject : MonoBehaviour
{
    private QSocket socket;

    public GameObject TextPrefab;
    public Transform ListViewArea;

    private void Awake()
    {
        var exists = GameObject.Find(nameof(ExecuteOnMainThread));
        if (exists == null)
        {
            new GameObject(nameof(ExecuteOnMainThread)).AddComponent<ExecuteOnMainThread>();
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

        socket.On(QSocket.EVENT_DISCONNECT, () =>
        {
            Debug.Log("EVENT_DISCONNECT");
            LogItem("Disconnected");
        });

        socket.On(QSocket.EVENT_CONNECT_ERROR, () => { Debug.Log("EVENT_CONNECT_ERROR"); });

        socket.On(QSocket.EVENT_RECONNECTING, () => { Debug.Log("EVENT_RECONNECTING"); });

        socket.On(QSocket.EVENT_CONNECT_TIMEOUT, () => { Debug.Log("EVENT_CONNECT_TIMEOUT"); });

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


        socket.Connect();
    }

    private void LogItem(string data)
    {
        ExecuteOnMainThread.RunOnMainThread(() =>
            {
                GameObject o = Instantiate(TextPrefab, ListViewArea, false);
                o.GetComponent<Text>().text = data;
                o.SetActive(true);
            }
        );
    }

    private void OnDestroy()
    {
        socket.Disconnect();
    }
}