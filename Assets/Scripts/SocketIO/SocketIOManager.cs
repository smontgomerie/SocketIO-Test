using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Socket.Quobject.EngineIoClientDotNet.Modules;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

namespace Scope.RemoteAR.SocketIO
{
    public class SocketIOManager
    {
        private QSocket socket;
        public event Action<float> OnSetValue;
        public event Action<string> OnSendMessage;
        public event Action<Dictionary<string, string>> OnJSONMessage;
        public event Action OnConnected; 
        public event Action OnDisconnected; 
        public event Action OnReconnecting;

        const bool LOG_ENABLED = true;
        
        public SocketIOManager(string url)
        {
            LogManager.Enabled = LOG_ENABLED;

            socket = IO.Socket(url, new IO.Options()
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
                OnConnected?.Invoke();
                socket.Emit("chat", "test");
                socket.Emit("welcome", "test");
                socket.Emit("hi", DateTime.Now.ToString());
            });

            socket.On("chat", data => { Debug.Log("data : " + data); });

            socket.On("web message", data =>
            {
                Debug.Log("web message : " + data);

                OnSendMessage?.Invoke(data as string);
            });

            socket.On("json", json =>
            {
                Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json as string);
                OnJSONMessage?.Invoke(dictionary);
            });

            socket.On("set value", data =>
            {
                Debug.Log("set value " + data);
                float result = 1;
                bool set = false;
                switch (data)
                {
                    case string t1:
                        if (float.TryParse(t1, out result))
                        {
                            set = true;
                        }

                        break;
                    case int t1:
                        result = t1;
                        set = true;
                        break;
                    case float t2:
                        result = t2;
                        set = true;
                        break;
                    case long t3:
                        result = t3;
                        set = true;
                        break;
                }

                if (set)
                {
                    OnSetValue?.Invoke(result);
                }
            });

            socket.On(QSocket.EVENT_DISCONNECT, (obj) =>
            {
                Debug.Log("EVENT_DISCONNECT reason: " + obj);
                OnDisconnected?.Invoke();
            });

            socket.On(QSocket.EVENT_CONNECT_ERROR, () =>
            {
                Debug.Log("EVENT_CONNECT_ERROR");
                OnSendMessage?.Invoke("EVENT_CONNECT_ERROR");
            });

            socket.On(QSocket.EVENT_RECONNECTING, () =>
            {
                Debug.Log("EVENT_RECONNECTING");
                OnSendMessage?.Invoke("Reconnecting");
                OnReconnecting?.Invoke();
            });

            socket.On(QSocket.EVENT_CONNECT_TIMEOUT, () =>
            {
                Debug.Log("EVENT_CONNECT_TIMEOUT");
                OnSendMessage?.Invoke("ConnectTimeout");
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
        }

        public void Disconnect()
        {
            socket.Disconnect();
        }
    }
}