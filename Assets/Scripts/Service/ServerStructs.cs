using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Requests
{
    MetaListRequest = 0,
    LevelDownloadRequest = 1,
    LevelCompleteRequest = 2,
    LevelUploadRequest = 3
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
}
[System.Serializable]
public struct LevelUploadRequest
{
    public int Version;
    public LevelMeta Meta;
    public string JsonData;
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
    public float Time;
    public int ID;
    public string Creator;
    public int Resource;
}
[System.Serializable]
public struct MetaFile
{
    public List<LevelMeta> Levels;
}
