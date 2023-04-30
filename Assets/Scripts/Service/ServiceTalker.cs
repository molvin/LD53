using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System;
using UnityEngine.XR;

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

    private Socket socket;

    private byte[] buffer = new byte[65536];

    private void OnGUI()
    {
        Dictionary<string, Action> funcs = new Dictionary<string, Action>();
        funcs.Add(ServerStructs.Requests.MetaListRequest.ToString(), () => GetMetaList());
        funcs.Add(ServerStructs.Requests.LevelCompleteRequest.ToString(), () => SendLevelComplete());
        funcs.Add(ServerStructs.Requests.LevelDownloadRequest.ToString(), () => DownloadLevel());
        funcs.Add(ServerStructs.Requests.LevelUploadRequest.ToString(), () => UploadLevel());

        int offset = 0;
        foreach (string name in Enum.GetNames(typeof(ServerStructs.Requests)))
        {
            if (GUI.Button(new Rect(20, 20 + offset, 150, 35), name))
            {
                funcs[name]();
            }
            offset += 50;
        }
    }

    public ServerStructs.MetaFile GetMetaList()
    {
        Connect();

        var request = new ServerStructs.MetaListRequest
        {
            Version = Version
        };
        Send(JsonUtility.ToJson(request), (byte)ServerStructs.Requests.MetaListRequest);

        string response = Receive();

        Debug.Log($"Response: {response}");

        return (ServerStructs.MetaFile) JsonUtility.FromJson(response, typeof(ServerStructs.MetaFile));
    }
    public void SendLevelComplete()
    {
        ServerStructs.MetaFile levelMeta = GetMetaList();
        ServerStructs.LevelMeta level = levelMeta.Levels[0];

        Connect();
        var request = new ServerStructs.LevelCompleteRequest
        {
            Version = Version,
            Level = level.ID,
            Success = 1
        };

        Send(JsonUtility.ToJson(request), (byte)ServerStructs.Requests.LevelCompleteRequest);

        string response = Receive();

        Debug.Log(response);
    }
    public void UploadLevel()
    {
        Connect();
        var request = new ServerStructs.LevelUploadRequest {
            Version = Version,
            Meta = new ServerStructs.LevelMeta{ },
            JsonData = "derå"
        };

        Send(JsonUtility.ToJson(request), (byte)ServerStructs.Requests.LevelUploadRequest);

        string response = Receive();

        Debug.Log(response);
    }
    public void DownloadLevel()
    {
        ServerStructs.MetaFile levelMeta = GetMetaList();
        ServerStructs.LevelMeta level = levelMeta.Levels[0];
        
        Connect();
        var request = new ServerStructs.LevelDownloadRequest
        {
            Version = Version,
            ID = level.ID,
            Creator = level.Creator
        };

        Send(JsonUtility.ToJson(request), (byte) ServerStructs.Requests.LevelDownloadRequest);

        string response = Receive();

        Debug.Log(response);
    }

    private void Connect()
    {
        IPAddress ip = IPAddress.Any;
        socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(new IPEndPoint(IPAddress.Parse(Ip), Port));
    }

    private void Send(string json, byte code)
    {
        MemoryStream stream = new MemoryStream();
        byte[] json_data = Encoding.Unicode.GetBytes(json);
        stream.Write(BitConverter.GetBytes(json_data.Length));
        stream.WriteByte(code);
        stream.Write(json_data);

        socket.Send(stream.ToArray());
    }

    private string Receive()
    {
        StringBuilder builder = new StringBuilder();

        try
        {
            int bytesRec = socket.Receive(buffer);
            int payloadSize = BitConverter.ToInt32(buffer, 0);
            int totalRecived = bytesRec - 4;
            builder.Append(Encoding.Unicode.GetString(buffer, 4, bytesRec - 4));
            while (totalRecived < payloadSize)
            {
                bytesRec = socket.Receive(buffer);
                totalRecived += bytesRec;
                builder.Append(Encoding.Unicode.GetString(buffer, 0, bytesRec));
            }
        }
        catch
        {
            Debug.LogError("failed to read response");
        }
        return builder.ToString();
    }
}
