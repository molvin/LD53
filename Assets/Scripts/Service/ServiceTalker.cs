using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System;
using UnityEngine.XR;
using static Serializer;

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
    public bool DebugGui;

    private Socket socket;

    private byte[] buffer = new byte[65536];

    private void OnGUI()
    {
        if (!DebugGui)
            return;

        Dictionary<string, Action> funcs = new Dictionary<string, Action>();
        funcs.Add(Requests.MetaListRequest.ToString(), () => GetMetaList());
        funcs.Add(Requests.LevelCompleteRequest.ToString(), () => SendLevelComplete(GetMetaList().Levels[0], 0));
        funcs.Add(Requests.LevelDownloadRequest.ToString(), () => DownloadLevel(GetMetaList().Levels[0]));
        funcs.Add(Requests.LevelUploadRequest.ToString(), () => UploadLevel(new LevelMeta(), "temp data"));

        int offset = 0;
        foreach (string name in Enum.GetNames(typeof(Requests)))
        {
            if (GUI.Button(new Rect(20, 20 + offset, 150, 35), name))
            {
                funcs[name]();
            }
            offset += 50;
        }
    }

    public MetaFile GetMetaList()
    {
        Connect();

        var request = new MetaListRequest
        {
            Version = Version
        };
        Send(JsonUtility.ToJson(request), (byte)Requests.MetaListRequest);

        string response = Receive();

        Debug.Log($"Response: {response}");

        return (MetaFile) JsonUtility.FromJson(response, typeof(MetaFile));
    }
    public void SendLevelComplete(LevelMeta meta, int success)
    {
        GetMetaList();

        Connect();
        var request = new LevelCompleteRequest
        {
            Version = Version,
            ID = meta.ID,
            Creator = meta.Creator,
            Success = success
        };

        Send(JsonUtility.ToJson(request), (byte)Requests.LevelCompleteRequest);

        string response = Receive();

        Debug.Log(response);
    }
    public void UploadLevel(LevelMeta meta, string json)
    {
        Connect();
        var request = new LevelUploadRequest {
            Version = Version,
            Meta = meta,
            JsonData = json
        };

        Debug.Log($"Uploading {meta.Creator} {meta.ID}");
        Debug.Log(json);

        Send(JsonUtility.ToJson(request), (byte)Requests.LevelUploadRequest);

        string response = Receive();

        Debug.Log(response);
    }
    public LevelData DownloadLevel(LevelMeta meta)
    {
        Connect();
        var request = new LevelDownloadRequest
        {
            Version = Version,
            ID = meta.ID,
            Creator = meta.Creator
        };

        Send(JsonUtility.ToJson(request), (byte) Requests.LevelDownloadRequest);

        string response = Receive();

        return (LevelData) JsonUtility.FromJson(response, typeof(LevelData));
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
