using System;
using System.Collections;
using System.Collections.Generic;
using Socket.Quobject.EngineIoClientDotNet.Modules;
using Socket.Quobject.SocketIoClientDotNet.Client;
using UnityEngine;

public class TestObject : MonoBehaviour {
  private QSocket socket;

  void Start ()
  {
    LogManager.Enabled = true;
    
    Debug.Log ("start");
    socket = IO.Socket ("http://192.168.86.238:11002/", new IO.Options()
    {
      Query = new Dictionary<string, string>
      {
        {"token", "V2" }
      },
    });

    socket.On (QSocket.EVENT_CONNECT, () => {
      Debug.Log ("Connected");
      socket.Emit ("chat", "test");
      socket.Emit ("hi", DateTime.Now.ToString());
    });

    socket.On ("chat", data => {
      Debug.Log ("data : " + data);
    });
    
    socket.On (QSocket.EVENT_CONNECT_ERROR, () => {
      Debug.Log ("EVENT_CONNECT_ERROR");
    });

    socket.On (QSocket.EVENT_RECONNECTING, () => {
      Debug.Log ("EVENT_RECONNECTING");
    });
    
    socket.On (QSocket.EVENT_CONNECT_TIMEOUT, () => {
      Debug.Log ("EVENT_CONNECT_TIMEOUT");
    });

    socket.On("hi", response =>
    {
      // Console.WriteLine(response.ToString());
      Debug.Log(response);
    });

    socket.Connect();

  }

  private void OnDestroy () {
    socket.Disconnect ();
  }
}