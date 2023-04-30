using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerStructs
{
    public struct MetaListRequest
    {
        public int Version { get; set; }
    }
    public struct LevelDownloadRequest
    {
        public int Version { get; set; }
        public int ID { get; set; }
        public string Creator { get; set; }
    }
    public struct LevelCompleteRequest
    {
        public int Version { get; set; }
        public int Level { get; set; }
        public int Success { get; set; }
    }
    public struct LevelUploadRequest
    {
        public int Version { get; set; }
        public LevelMeta Meta { get; set; }
        public string JsonData { get; set; }
    }
    public struct Level
    {
        public LevelMeta Meta { get; set; }
        public string JsonData { get; set; }
    }
    public struct LevelMeta
    {
        public int Wins { get; set; }
        public int Attempts { get; set; }
        public float Time { get; set; }
        public int ID { get; set; }
        public string Creator { get; set; }
        public int Resource { get; set; }
    }
    public struct MetaFile
    {
        public List<LevelMeta> Levels { get; set; }
    }
}
