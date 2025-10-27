using System.Collections.Generic;
using UnityEngine;

public class PatrolState : BaseState
{
    // track which waypoint we are currently targeting
    public int waypointIndex;
    public float waitTimer;

    public override void Enter()
    {

    }

    public override void Perform()
    {
        PatrolCycle();
    }

    public override void Exit()
    {

    }

    public void PatrolCycle()
    {
        // implement patrol logic here
        if (npc.Agent.remainingDistance < 0.2f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer > 3)
            {
                if (waypointIndex < npc.path.waypoints.Count - 1)
                    waypointIndex++;
                else
                    waypointIndex = 0;
                npc.Agent.SetDestination(npc.path.waypoints[waypointIndex].position);
                waitTimer = 0;
            }
        }
    } 
}
