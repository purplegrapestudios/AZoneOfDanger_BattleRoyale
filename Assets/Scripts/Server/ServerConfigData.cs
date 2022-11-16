using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConfigData : MonoBehaviour
{
    
    public static string IPAddress;
    public static ushort Port;
    public static int TargetFrameRate;
    public static string ServerName;
    public static int PlayModeInt;
    public static PlayMode PlayMode { get { return (PlayMode)PlayModeInt; } }
    public static int MapIndexInt;
    public static MapIndex MapIndex { get { return (MapIndex)MapIndexInt; } }
    public static int MaxPlayers;

}
