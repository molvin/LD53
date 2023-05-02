using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Requests
{
    MetaListRequest = 0,
    LevelDownloadRequest = 1,
    LevelCompleteRequest = 2,
    LevelUploadRequest = 3,
    LevelMoveRequest = 4
}

[System.Serializable]
public struct MetaListRequest
{
    public int Version;
}
[System.Serializable]
public struct LevelDownloadRequest
{
    public int Version;
    public int ID;
    public string Creator;
}
[System.Serializable]
public struct LevelCompleteRequest
{
    public int Version;
    public int ID;
    public string Creator;
    public int Success;
    public float Time;
    public string RecordName;
}
[System.Serializable]
public struct LevelUploadRequest
{
    public int Version;
    public LevelMeta Meta;
    public string JsonData;
}
[System.Serializable]
public struct LevelMoveRequest
{
    public int Version;
    public int ID;
    public string Creator;
    public int Index;
}
[System.Serializable]
public struct Level
{
    public LevelMeta Meta;
    public string JsonData;
}
[System.Serializable]
public struct LevelMeta
{
    public int Wins;
    public int Attempts;
    public float AuthorTime;
    public float RecordTime;
    public string RecordName;
    public int ID;
    public string Creator;
    public int Resource;
}
[System.Serializable]
public struct MetaFile
{
    public List<LevelMeta> Levels;
}
