using System.Collections.Generic;
using UnityEngine;

public class BehaviourRules
{
    /// <summary>
    /// This method create a random force (2 dimensions).
    /// It aim to allow an agent to move randomly.
    /// The greater the intensity, the greater the force.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <returns> A random vector, representing a random force.</returns>
    public static Vector3 RandomMovement(float intensity, SerializableRandom generator)
    {
        float alea = 0.1f;
        if (generator.Rand(0,1) < alea)
        {
            float x = (float)generator.Rand(0, 1) - 0.5f;
            float z = (float)generator.Rand(0, 1) - 0.5f;
            Vector3 force = new Vector3(x, 0.0f, z);
            force.Normalize();

            //Modification de la puissance de cette force
            force *= intensity;

            return force;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// This method create a force leading forward (2 dimensions).
    /// It aim to allow an agent to move forward.
    /// The greater the intensity, the greater the force.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <param name="direction"> The desired direction of the resulted force.</param>
    /// <returns>A force leading in the same direction as the direction in parameters.</returns>
    public static Vector3 MoveForward(float intensity, Vector3 direction)
    {
        Vector3 force = direction;
        force *= intensity;
        return force;
    }

    /// <summary>
    /// This method create a force opposite to the current speed.
    /// It aim to reduce the agent speed, depending of the friction intensity.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force. </param>
    /// <param name="speed"></param>
    /// <returns>A force opposite to the speed in parameters.</returns>
    public static Vector3 Friction(float intensity, Vector3 speed)
    {
        float k = intensity;
        if (intensity > 1.0f)
        {
            k = 1.0f;
            Debug.Log("Friction intensity higher than expected (> 1.0f)");
        } 
        else if(intensity<0.0f)
        {
            k = 0.0f;
            Debug.Log("Friction intensity lower than expected (< 0.0f)");
        }
        Vector3 force = speed;
        force *= -k;
        return force;
    }

    /// <summary>
    /// This method create a force opposite to the neighbours that are too close.
    /// It aim to avoid collision between agents.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <param name="position"> The position of the agent that will receive the force.</param>
    /// <param name="neighboursPositions"> The positions of the agent's neigbours.</param>
    /// <param name="maxSpeed"> The maximum movement speed that can be reach by an agent.</param>
    /// <param name="safetyDistance"> The security distance that must be crossed before the rule is applied.</param>
    /// <returns>A force pulling away from close neighbours.</returns>
    public static Vector3 AvoidCollisionWithNeighbours(float intensity, Vector3 position, List<Vector3> neighboursPositions, float maxSpeed, float safetyDistance)
    {
        Vector3 totalForce = Vector3.zero;

        foreach (Vector3 p in neighboursPositions)
        {
            if (Vector3.Distance(p, position) <= safetyDistance)
            {
                Vector3 force = position - p;
                force.Normalize();
                force *= intensity * maxSpeed;
                totalForce += force;
            }
        }
        return totalForce;
    }


    /// <summary>
    /// Create a cohesion force based on neighbours positions. This force brings a position closer to its neighbours.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force. </param>
    /// <param name="position"> The position of the agent that will receive the force. </param>
    /// <param name="neighboursPositions"> The positions of the agent's neigbours. </param>
    /// <returns> A force bringing a position closer to its neighbours.</returns>
    public static Vector3 Cohesion(float intensity, Vector3 position, List<Vector3> neighboursPositions)
    {
        int count = 0;
        Vector3 g = Vector3.zero;
        foreach (Vector3 p in neighboursPositions)
        {
            count += 1;
            g += p;
        }
        if (count > 0)
        {
            g /= count;
            Vector3 force = g - position;
            force *= intensity;
            g.y = 0.0f; //To stay in 2D
            return force;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Create a separation force based on neighbours positions. This force moves a position away to its neighbours.
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <param name="position"> The position of the agent that will receive the force. </param>
    /// <param name="neighboursPositions"> The positions of the agent's neigbours. </param>
    /// <returns> A force moving a position away to its neighbours. </returns>
    public static Vector3 Separation(float intensity, Vector3 position, List<Vector3> neighboursPositions)
    {
        int count = 0;
        Vector3 totalForce = Vector3.zero;

        foreach (Vector3 p in neighboursPositions)
        {
            count += 1;
            Vector3 force = position - p;
            force.Normalize();

            totalForce += force;
        }

        if (count > 0)
        {
            totalForce /= count;
            totalForce *= intensity;
            totalForce.y = 0.0f; //To stay in 2D
            return totalForce;
        }
        else
        {
            return Vector3.zero;
        }
    }


    /// <summary>
    /// Create an alignment force based on current neighbours speeds. This force align speed to match its neighbours speed (direction and intensity).
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <param name="neighboursSpeeds"> The speeds of the agent's neigbours. </param>
    /// <returns> A force aligning a speed to its neighbours. </returns>
    public static Vector3 Alignment(float intensity, List<Vector3> neighboursSpeeds)
    {
        int count = 0;
        Vector3 vm = Vector3.zero;

        foreach (Vector3 v in neighboursSpeeds)
        {
                count += 1;
                vm += v;
        }

        if (count > 0)
        {
            vm /= count;
            vm *= intensity;
            vm.y = 0.0f; //To stay in 2D

            return vm;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Create an alignment force based on difference between current neighbours speeds and agen current speed.
    /// This force align speed to match its neighbours speed (direction and intensity).
    /// </summary>
    /// <param name="intensity"> The intensity of the resulted force.</param>
    /// <param name="speed"> The speed of the agent.</param>
    /// <param name="neighboursSpeeds"> The speeds of the agent's neigbours.</param>
    /// <returns>  A force aligning a speed to its neighbours. </returns>
    public static Vector3 AlignmentUsingDifference(float intensity, Vector3 speed, List<Vector3> neighboursSpeeds)
    {
        int count = 0;
        Vector3 a = Vector3.zero;

        foreach (Vector3 s in neighboursSpeeds)
        {
            count += 1;
            a += (speed - s);
        }

        if (count > 0)
        {
            a.y = 0.0f; //To stay in 2D
            a *= intensity;
            a = -a;
        }
        return a;
    }

    /// <summary>
    /// Create a bouncing force that push back inside the area. This force allow to keep the position inside the area.
    /// </summary>
    /// <param name="position"> The position of the agent that will receive the force. </param>
    /// <param name="maxSpeed"> The maximum allowed speed of the agent. </param>
    /// <param name="mapSizeX"> The size (in x axes) of the map in meters.</param>
    /// <param name="mapSizeZ"> The size (in z axes) of the map in meters.</param>
    /// <param name="safetyDistance"> The distance from the wall at which the rule will starts to be applied. </param>
    /// <returns> A force pushing back inside the map limits.</returns>
    public static Vector3 BouncesOffWall(Vector3 position, float maxSpeed, float mapSizeX, float mapSizeZ, float safetyDistance = 0.3f)
    {
        float x = 0.0f;
        float z = 0.0f;
   
        if (position.x > mapSizeX - safetyDistance)
        {
            float dist = Mathf.Abs(mapSizeX - position.x);
            x = -maxSpeed * safetyDistance / dist;
        }
        if (position.x < safetyDistance)
        {
            float dist = Mathf.Abs(position.x);
            x = maxSpeed * safetyDistance / dist;
        }

        if (position.z > mapSizeZ - safetyDistance)
        {
            float dist = Mathf.Abs(mapSizeZ - position.z);
            z = -maxSpeed * safetyDistance / dist;
        }
        if (position.z < safetyDistance)
        {
            float dist = Mathf.Abs(position.z);
            z = maxSpeed * safetyDistance / dist;
        }
        Vector3 rebond = new Vector3(x, 0.0f, z);

        return rebond;
    }


    /// <summary>
    /// Create a force that maintains a specific distance between positions, based on current neighbours.
    /// </summary>
    /// <param name="distanceBetweenAgents"> The desired distance. </param>
    /// <param name="position"> The position of the agent that will receive the force. </param>
    /// <param name="neighboursPositions"> The positions of the agent's neigbours. </param>
    /// <returns> A force that maintains a specific distance between positions.</returns>
    public static Vector3 PotentialFunction(float distanceBetweenAgents, Vector3 position, List<Vector3> neighboursPositions)
    {
        int count = 0;

        Vector3 g = Vector3.zero;
        foreach (Vector3 p in neighboursPositions)
        {

            Vector3 rij = position - p;
            rij /= distanceBetweenAgents;
            float uX = (-2 * position.x * rij.x + 2 * position.x * rij.x * (Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2))) / Mathf.Pow((Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2)), 2);
            float uZ = (-2 * position.z * rij.z + 2 * position.x * rij.z * (Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2))) / Mathf.Pow((Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2)), 2);

            g += new Vector3(uX, 0.0f, uZ);

            count ++;
        }
        return -g;
    }

}
