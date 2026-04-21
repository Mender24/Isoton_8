using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class YapGroup
{
    public string Name;
    [Min(0.1f)] public float Weight = 1f;
    public float GroupCooldown = 0f;
    public List<CellAudioClip> Clips = new();

    [NonSerialized] public float CooldownTimer;
}
