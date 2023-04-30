using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class ServiceTalker : MonoBehaviour
{
    /*
     * Get metadata
     * level request
     * level completed
     * level upload
     */

    public string Ip = "192.168.1.241";
    public int Port = 5302;
    public int Version = 0;

    public Socket socket;

    private void Start()
    {
        IPAddress ip = IPAddress.Any;
        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(Ip), Port);
        socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ep);
    }

    private void OnGUI()
    {
        if(GUI.Button(new Rect(20, 20, 80, 35), "Get Meta"))
        {
            GetMeta();
        }
    }

    public void GetMeta()
    {
        var request = new ServerStructs.MetaListRequest
        {
            Version = Version
        };
        string json = JsonUtility.ToJson(request);

    }

}
