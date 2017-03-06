using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Menu : MonoBehaviour {

    public string IP = "127.0.0.1";
    public int port = 6777;
    

    private void OnGUI()
    {
        
        if(Network.peerType == NetworkPeerType.Disconnected)
        {
            if(GUI.Button(new Rect(100,100,100,25), "Strat Client"))
            {
                Network.Connect(IP, port);
            }
            if (GUI.Button(new Rect(100, 125, 100, 25), "Strat Server"))
            {
                var useNat = !Network.HavePublicAddress();
                Network.InitializeServer(10, port, useNat);
            }
        }else
        {
            if(Network.peerType == NetworkPeerType.Server)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Server");
                GUI.Label(new Rect(100, 125, 100, 25), "Connections: "+Network.connections.Length);

                if (GUI.Button(new Rect(100, 150, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }
            }
            if(Network.peerType == NetworkPeerType.Client)
            {
                GUI.Label(new Rect(100, 100, 100, 25), "Clinet");

                if(GUI.Button(new Rect(100, 125, 100, 25), "Logout"))
                {
                    Network.Disconnect(250);
                }
            }
        }
    }

  
}
