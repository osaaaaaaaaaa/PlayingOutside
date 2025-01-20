using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioVolume
{
    static float bgmVolume = 1f;
    public static float BgmVolume { get { return bgmVolume; } set { bgmVolume = value; } }
    static float seVolume = 1f;
    public static float SeVolume { get {return seVolume; } set { seVolume = value; } }
}
