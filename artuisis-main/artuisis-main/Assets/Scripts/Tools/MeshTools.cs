using System.Collections.Generic;
using UnityEngine;

public class MeshTools
{
    public static List<Vector3> TranformLineToRectanglePoints(Vector3 startPoint, Vector3 endPoint, float width)
    {
        //Calculer la normale à la ligne en paramètre

        Vector3 line = endPoint - startPoint;

        Vector3 perp = new Vector3(-line.z, 0.0f, line.x);
        perp = perp.normalized * (width / 2);


        List<Vector3> points = new List<Vector3>();
        points.Add(startPoint - perp);
        points.Add(endPoint - perp);
        points.Add(endPoint + perp);
        points.Add(startPoint + perp);

        return points;
    }

    #region Methods - Cuboid mesh
    public static System.Tuple<List<Vector3>, List<int>> TranformLineToCuboidPoints(Vector3 startPoint, Vector3 endPoint, float width,float height)
    {
        //Calculer la normale à la ligne en paramètre

        Vector3 line = endPoint - startPoint;
        Vector3 up = Vector3.up * (height / 2);

        Vector3 perp = new Vector3(-line.z, 0.0f, line.x);
        perp = perp.normalized * (width / 2);



        Vector3 point0 = startPoint + up;
        Vector3 point1 = startPoint - perp;
        Vector3 point2 = startPoint - up;
        Vector3 point3 = startPoint + perp;

        Vector3 point4 = endPoint + up;
        Vector3 point5 = endPoint - perp;
        Vector3 point6 = endPoint - up;
        Vector3 point7 = endPoint + perp;

        List<Vector3> points = new List<Vector3>();

        points.Add(point0);
        points.Add(point1);
        points.Add(point2);
        points.Add(point3);
        points.Add(point4);
        points.Add(point5);
        points.Add(point6);
        points.Add(point7);


        List<int> triangles = new List<int>();

        triangles.Add(0);
        triangles.Add(3);
        triangles.Add(4);

        triangles.Add(4);
        triangles.Add(3);
        triangles.Add(7);

        triangles.Add(7);
        triangles.Add(3);
        triangles.Add(2);

        triangles.Add(7);
        triangles.Add(2);
        triangles.Add(6);

        triangles.Add(6);
        triangles.Add(1);
        triangles.Add(5);

        triangles.Add(6);
        triangles.Add(2);
        triangles.Add(1);

        triangles.Add(5);
        triangles.Add(0);
        triangles.Add(4);

        triangles.Add(5);
        triangles.Add(1);
        triangles.Add(0);

        triangles.Add(4);
        triangles.Add(7);
        triangles.Add(5);

        triangles.Add(7);
        triangles.Add(6);
        triangles.Add(5);

        triangles.Add(3);
        triangles.Add(1);
        triangles.Add(2);

        triangles.Add(1);
        triangles.Add(3);
        triangles.Add(0);

        return new System.Tuple<List<Vector3>, List<int>>(points, triangles);
    }



    #endregion

    #region Methods - Spring mesh
    public static System.Tuple<List<Vector3>, List<int>> GetSpringMesh(Vector3 position1, Vector3 position2, int nbLoops, int nbVerticesPerLoops, float width, float wireWidth)
    {
        float halfWidth = width / 2.0f;
        Vector3 circleNormal = Vector3.up;

        Vector3 springVector = position2 - position1;
        Vector3 springNormal = springVector.normalized;

        Quaternion rotation = Quaternion.FromToRotation(circleNormal, springNormal);

        float angle = 2 * Mathf.PI / (float)nbVerticesPerLoops;

        Vector3 springStep = springVector / (nbVerticesPerLoops * nbLoops);

        List<Vector3> circlePoints = new List<Vector3>();

        for (int i = 0; i < nbVerticesPerLoops; i++)
        {
            float currentAngle = angle * i;
            Vector3 vertex = new Vector3(Mathf.Cos(currentAngle) * halfWidth, 0.0f, Mathf.Sin(currentAngle) * halfWidth) + position1;
            circlePoints.Add(RotatePointAroundPivot(vertex, position1, rotation));
        }

        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < nbLoops * nbVerticesPerLoops; i++)
        {
            points.Add(circlePoints[i % nbVerticesPerLoops] + springStep * i);
        }

        //Initialise lists of vertices and triangles for the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count - 1; i++)
        {
            //Get the vertices of the line
            System.Tuple<List<Vector3>, List<int>> m = MeshTools.TranformLineToCuboidPoints(points[i], points[(i + 1)], wireWidth, wireWidth);

            List<Vector3> v = m.Item1;
            List<int> t = m.Item2;

            //Update triangles indexes before adding them to the triangles list
            for (int k = 0; k < t.Count; k++)
            {
                t[k] += vertices.Count;
            }

            //Updathe vertices and triangles list with the new line
            vertices.AddRange(v);
            triangles.AddRange(t);
        }

        return new System.Tuple<List<Vector3>, List<int>>(vertices, triangles);
    }
    #endregion


    //https://discussions.unity.com/t/rotate-a-vector-around-a-certain-point/81225/2
    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Quaternion rotation)
    {
        return rotation * (point - pivot) + pivot;
    }



    /// <summary>
    /// From a list of vertices, prepare the triangles array that will be use in a mesh. 
    /// .......
    /// </summary>
    /// <param name="points"> The list of vertices, with at 0 the origin of each triangles</param>
    /// <returns> The list of index to form mesh triangles</returns>
    public static List<int> DrawFilledTriangles(Vector3[] points)
    {
        int triangleAmount = points.Length - 2;
        List<int> newTriangles = new List<int>();
        for (int i = 0; i < triangleAmount; i++)
        {
            newTriangles.Add(0);
            newTriangles.Add(i + 2);
            newTriangles.Add(i + 1);
        }
        return newTriangles;
    }
}
