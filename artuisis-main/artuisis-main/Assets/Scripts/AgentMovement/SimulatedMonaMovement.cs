using System;
using Unity.VisualScripting;
using UnityEngine;

public class SimulatedMonaMovement
{
    /// <summary>
    ///This method simulates the movement of a mobile robot on two wheels, with the intensity of each wheel adjustable. 
    ///It calculates the next position of the robot from an initial position and orientation, as well as from the intensity of the motors.
    /// </summary>
    /// <param name="position">The initial position of the robot, halfway between the two wheels</param>
    /// <param name="direction">The initial position of the robot</param>
    /// <param name="leftIntensity">Left wheel motor intensity</param>
    /// <param name="rightIntensity">Right wheel motor intensity</param>
    /// <param name="elapsedTime">Time elapsed to next position</param>
    /// <param name="maxSpeed">Max speed of the robot</param>
    /// <param name="monaWidth">Distance between the two wheels</param>
    /// <returns>The new position and direction of the robot</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static Tuple<Vector3, Vector3> Move(Vector3 position, Vector3 direction, float leftIntensity, float rightIntensity, float elapsedTime, float maxSpeed = 0.1f, float monaWidth = 0.08f)
    {
        //A few checks
        if (leftIntensity > 1 || leftIntensity < -1) { throw new ArgumentOutOfRangeException("leftIntensity is outside [-1,1],"+ leftIntensity); }
        if(rightIntensity > 1 || rightIntensity < -1) { throw new ArgumentOutOfRangeException("rightIntensity is outside [-1,1]," + rightIntensity); }

        if(leftIntensity == 0 && rightIntensity == 0) return Tuple.Create(position, direction); //If the wheels don't move, neither does the mona

        //90° in radian
        float angleRad = 1.5707963268f;
        Vector2 v2Position = new Vector2(position.x, position.z);
        Vector2 v2Direction = new Vector2(direction.x, direction.z);

        //Calculate the distance covered by each wheel
        float leftDistance = leftIntensity * maxSpeed * elapsedTime;
        float rightDistance = rightIntensity * maxSpeed * elapsedTime;

        

        //Calculate the intitial position of each wheel, based on mona position and direction
        Vector2 rotatedDirection = (GeometryTools.Rotate(v2Position, v2Position + v2Direction, angleRad) - v2Position).normalized;

        Vector2 positionLeftWheel = v2Position + rotatedDirection * (monaWidth / 2.0f);
        Vector2 positionRightWheel = v2Position - rotatedDirection * (monaWidth / 2.0f);

        //We now want to obtain the positions of the wheels after they have moved
        Vector2 newPositionLeftWheel = Vector3.zero;
        Vector2 newPositionRightWheel = Vector3.zero;

        //if (Math.Abs(leftDistance) == Math.Abs(rightDistance))
        if (leftDistance == rightDistance) //
        {
            newPositionLeftWheel = positionLeftWheel + (v2Direction.normalized * leftDistance);
            newPositionRightWheel = positionRightWheel + (v2Direction.normalized * rightDistance);
        }
        else //If the distances travelled are different, the mona will not go straight ahead and there will be an offset on one side.
        {
            float DE = monaWidth;
            float BE, CD, AE, DA; //BE Short distance, CD Big distance
            Vector2 D,E,A;

            //Thales' theorem is used with two triangles, ACD and ABE respectively.
            //The straight line DE corresponds to the initial position of the robot, and D and E to the position of its wheels.
            //The straight line CB corresponds to the calculated position of the robot, C and B the future position of its wheels.
            //The point A corresponds to the virtual pivot point around which the robot rotates.
            //According to Thales' theorem, ABE is included in ACD if B lies on line AC and E lies on line AE.
            //Otherwise, we get two triangles that meet at A, and so A belongs to line ED, and also to line BC.
            //Depending on the intensity of the wheels, we get 8 different triangles, depending on which wheel is going faster, whether they are going in different directions and the direction of rotation.
            bool leftBigger;
            if (Math.Abs(leftDistance) > Math.Abs(rightDistance))
            {
                leftBigger = true;
                BE = Mathf.Abs(rightDistance);
                CD = Mathf.Abs(leftDistance);
                D = positionLeftWheel;
                E = positionRightWheel;
            } else 
            { 
                leftBigger = false;
                BE = Mathf.Abs(leftDistance);
                CD = Mathf.Abs(rightDistance);
                D = positionRightWheel;
                E = positionLeftWheel;
            }


            //Thales formula is CD/BE = DA/AE
            //If the shape intersects, and the point therefore lies between D and E (the two wheels),
            //then AE is subtracted from the formula to calculate DA instead of being added to DE.
            if ((leftDistance * rightDistance) < 0.0f) 
            { 
                //Here, DA = DE - AE
                AE = DE / (CD / BE + 1);
                DA = DE - AE;
            } else
            {
                //Here, DA = DE + AE
                AE = DE / (CD / BE - 1);
                DA = DE + AE;
            }

            Vector2 dir = (E - D).normalized;


            //The position of point A is calculated because the robot rotates around.
            A = D + dir * DA;

            //The angle of rotation about point A is calculated using Pythagoras (based on an isosceles triangle ACD cut in half)
            float angle = Mathf.Asin((CD / 2) / DA) * 2;

            //As GeometryTools' Rotate method takes an angle in counter-clockwise logic,
            //the angle must be reversed if the rotation is clockwise.
            if ((leftBigger && leftDistance > 0) || (!leftBigger && rightDistance < 0))
            {
                //Clockwise
                angle = -angle;
            }

            //To obtain the new position of the wheels, they are rotated around A according to the value of Angle.
            newPositionLeftWheel = GeometryTools.Rotate(A, positionLeftWheel, angle);
            newPositionRightWheel = GeometryTools.Rotate(A, positionRightWheel, angle);

        }


        //Calculate new position (and convert it to Vector3)
        Vector2 newV2Position = (newPositionLeftWheel + newPositionRightWheel) / 2.0f;
        Vector3 newPosition = new Vector3(newV2Position.x, 0.0f, newV2Position.y);

        //Calculate new direction (and convert it to Vector3)
        Vector2 newV2Direction = (GeometryTools.Rotate(newV2Position, newPositionRightWheel, angleRad) - newV2Position).normalized;
        Vector3 newDirection = new Vector3(newV2Direction.x, 0.0f, newV2Direction.y);


        //These logs are used to check that the actual distances covered by the wheels correspond to the desired distances,
        //by showing the difference between the two, which is supposed to be tiny.
        // Debug.Log("Roue droite : " + Mathf.Abs((newPositionRightWheel - positionRightWheel).magnitude - rightDistance));
        // Debug.Log("Roue gauche : " + Mathf.Abs((newPositionLeftWheel - positionLeftWheel).magnitude - leftDistance));

        return Tuple.Create(newPosition,newDirection);
    }
}
