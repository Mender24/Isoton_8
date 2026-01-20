using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsPlayerInLineOfSight", story: "[Agent] sees [Player] in line of sight", category: "Conditions", id: "6eaef50312d35f04e852c2b98ef9bb58")]
public partial class IsPlayerInLineOfSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BehaviorGraphAgent BehaviorAgent;
    [SerializeReference] public float Angle;
    [SerializeReference] public float LenVision;
    [SerializeReference] public float Height;

    public override bool IsTrue()
    {
        Vector3 rayDirection = (Player.Value.transform.position - Agent.Value.transform.position).normalized;
        float rayDistance = Vector3.Distance(Agent.Value.transform.position, Player.Value.transform.position);
        float angle = Math.Abs(Vector3.Angle(Agent.Value.transform.forward, rayDirection));

        bool isDebug = false;
        if(BehaviorAgent.GetVariable("DebugVision", out BlackboardVariable isDebugValue))
            isDebug = (bool)isDebugValue.ObjectValue;
        else
            Debug.Log("No bool 'DebugVision'");

        if(isDebug)
        {
            Vector3 vectorPos = Agent.Value.transform.position + new Vector3(0, Height, 0);

            if(angle <= Angle && rayDistance <= LenVision)
                Debug.DrawRay(vectorPos, rayDirection * rayDistance, Color.red);
            else
                Debug.DrawRay(vectorPos, rayDirection * rayDistance, Color.white);
            
            Debug.DrawRay(vectorPos, RotateVector(Angle, Agent.Value.transform.forward) * LenVision, Color.blue);
            Debug.DrawRay(vectorPos, RotateVector(-Angle, Agent.Value.transform.forward) * LenVision, Color.blue);
        }

        bool InSight = rayDistance > LenVision || angle > Angle;

        if(InSight)
        {
            // if(isDebug)
            //     Debug.Log("Not detected");

            return false;
        }

        RaycastHit hit;

        if (Physics.Raycast(Agent.Value.transform.position, rayDirection, out hit, rayDistance))
        {
            // if(isDebug)
            //     Debug.Log("Detected");

            if(!BehaviorAgent.SetVariableValue("HasSeenLastTarget", true))
                Debug.Log("No bool 'HasSeenLastTarget'");

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnStart()
    {
        BehaviorAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();

        if(BehaviorAgent.GetVariable("Angle", out BlackboardVariable angle))
            Angle = (float)angle.ObjectValue;

        if(BehaviorAgent.GetVariable("LenVision", out BlackboardVariable len))
            LenVision = (float)len.ObjectValue;   

        if(BehaviorAgent.GetVariable("Height", out BlackboardVariable hight))
            Height = (float)hight.ObjectValue;    
    }

    public override void OnEnd()
    {
    }

    private Vector3 RotateVector(float angle, Vector3 vector) 
    {
        return new
        (
            (float)Math.Cos(angle * Mathf.Deg2Rad) * vector.x - vector.z * (float)Math.Sin(angle * Mathf.Deg2Rad),
            0,
            (float)Math.Sin(angle * Mathf.Deg2Rad) * vector.x + vector.z * (float)Math.Cos(angle * Mathf.Deg2Rad)
        );
    } 
}
