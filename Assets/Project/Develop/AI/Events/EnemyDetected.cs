using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/EnemyDetected")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "EnemyDetected", message: "[Agent] has spotted [Player]", category: "Events", id: "75bc86a022572b0093017d7d8c8c703c")]
public sealed partial class EnemyDetected : EventChannel<GameObject, GameObject> { }

