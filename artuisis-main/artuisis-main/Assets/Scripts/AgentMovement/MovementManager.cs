using System;
using UnityEngine;
using static MovementManager;

public class MovementManager
{
    public enum AgentMovement
    {
        Particle,   //Use the basic particle system, without physical constraints
        MonaRobot   //Use the mona robot movement system, using 2 wheels and physical characteristics
    }

    public static Tuple<Vector3,Vector3> ApplyAgentMovement(AgentData agent, SwarmParameters parameters, float elapsedTime)
    {
        Tuple<Vector3,Vector3> newPositionAndDirection;
        AgentMovement agentMovement = parameters.GetAgentMovement();
        switch (agentMovement)
        {
            case AgentMovement.Particle:
                newPositionAndDirection = ParticuleMovement(agent, parameters, elapsedTime);
                break;
            default:
                newPositionAndDirection = MonaMovement(agent, parameters, elapsedTime);
                break;
        }
        return newPositionAndDirection;
    }

    private static Tuple<Vector3, Vector3> ParticuleMovement(AgentData agent, SwarmParameters parameters ,float elapsedTime)
    {
        Vector3 newPosition = agent.GetPosition();
        newPosition += agent.GetSpeed() * elapsedTime;

        Tuple<Vector3, Vector3> newPositionAndDirection = new Tuple<Vector3, Vector3>(newPosition, agent.GetSpeed().normalized);

        //A vector with a magnitude of 0 cannot represent a direction.
        //If this happens, the previous direction is kept.
        float magnitude = Vector3.Magnitude(newPositionAndDirection.Item2);

        if(magnitude == 0)
        {
            newPositionAndDirection = Tuple.Create(newPositionAndDirection.Item1, agent.GetDirection());
        }
        return newPositionAndDirection;
    }

    private static Tuple<Vector3, Vector3> MonaMovement(AgentData agent, SwarmParameters parameters, float elapsedTime)
    {
        //Negative angle (in degree) will be counter-clockwise rotation
        float angle = Vector3.SignedAngle(agent.GetDirection(), agent.GetSpeed(), Vector3.up);

        float threshold = 90.0f;

        float R, L;
        if(Mathf.Abs(angle) > threshold) //Between 180° (or -180°) and the threshold, the rotation power is at its maximum.
        {
            if(angle < 0)
            {
                L = -1.0f;
                R = 1.0f;
            } else
            {
                L = 1.0f ; 
                R = -1.0f;
            }
        } else //The robot start slowing to move forward. If the rotation is near the threshold, the rotation near its maximum.
        {
            float val = Mathf.Abs(angle / threshold);

            val = Mathf.Pow(val, 0.5f); //By using a square root, the transition will not be linear.
                                        //The closer the robot gets to the direction it needs to follow, the further it will advance.
                                        //As soon as it moves a little further away, the rotation quickly becomes stronger.

            if (angle < 0)
            {
                L = -1.0f * val + 1.0f * (1.0f-val);
                R = 1.0f;
            }
            else
            {
                L = 1.0f;
                R = -1.0f * val + 1.0f * (1.0f - val);
            }
        }

        //The intensity of the motors should also depend on the ‘speed’ force.
        //If this force is greater than the robot's maximum speed, the intensity is maximum.
        //Otherwise, the motor current is reduced proportionally.
        float speedMagnitude = Vector3.Magnitude(agent.GetSpeed()) / parameters.GetMaxSpeed();

        if (speedMagnitude < 1.0f)
        {
            L = L * speedMagnitude;
            R = R * speedMagnitude;
        }

        Tuple<Vector3, Vector3> newPositionAndDirection = SimulatedMonaMovement.Move(agent.GetPosition(), agent.GetDirection(), L, R, elapsedTime, parameters.GetMaxSpeed());

        return newPositionAndDirection;
    }

}
