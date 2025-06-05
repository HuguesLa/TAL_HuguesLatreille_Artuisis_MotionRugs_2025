using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is currently "deprecated" because it wasn't updated in a previous update of the simulator. 
/// However, it aims to provide a system to create, by drawing shape with the mouse, a forces map that can be use to influence swarm agents.
/// The code can be reused in a future update.
/// </summary>

public class PatternCreator : MonoBehaviour
{
    Camera mainCamera;

    List<Vector2> patternVertices;

    List<LineRenderer> patternRenderer;

    [SerializeField]
    private SwarmManager swarmManager;

    private bool isDrawing = false;


    //These two parameters should depend on the size of the simulator map.
    private float mapSizeX = 5.0f;
    private float mapSizeZ = 5.0f;


    private int cutNumber = 50; //Corresponds to the number of rows and columns in the vector field

    private bool[,] vectorFieldState;
    private Vector2[,] vectorField;
    // Start is called before the first frame update
    void Start()
    {
        mapSizeX = swarmManager.GetSwarmData().GetParameters().GetMapSizeX();
        mapSizeZ = swarmManager.GetSwarmData().GetParameters().GetMapSizeZ();

        mainCamera = Camera.main;
        patternRenderer = new List<LineRenderer>();

        vectorFieldState = new bool[cutNumber,cutNumber];
        vectorField = new Vector2[cutNumber,cutNumber];

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetButton("Fire1"))
        {
            if (!isDrawing) //Start to draw
            {
                isDrawing = true;
                patternVertices = new List<Vector2>();
                Debug.Log("I start drawing!");
            }

            Vector3 mousePos = Input.mousePosition;
            {
                Ray temp = mainCamera.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));

                float scale = temp.origin.y / temp.direction.y;

                Vector3 res = temp.GetPoint(-scale);

                //Round to the second decimal place
                res.x = Mathf.Round(res.x * 100f) / 100f;
                res.z = Mathf.Round(res.z * 100f) / 100f;


                Vector2 vertex = new Vector2(res.x, res.z);

                if (patternVertices.Count > 0)
                {
                    Vector2 lastVertex = patternVertices[patternVertices.Count - 1];
                    if (lastVertex.x != vertex.x || lastVertex.y != vertex.y)
                    {
                        patternVertices.Add(vertex);
                        Debug.Log(vertex);
                    }
                }
                else
                {
                    patternVertices.Add(vertex);
                    Debug.Log(vertex);
                }



            }
        }
        else
        {
            if (isDrawing) //Stopped drawing, start calculating the force field
            {
                isDrawing = false;
                Debug.Log("I finished!");
                Debug.Log(patternVertices.Count);
                ClearRenderer();
                //DrawPattern();
                CalculateVectorFieldState();
                CalculateVectorField();
                SmoothingVectorField();
                //DrawVectorField();
            }
        }

        if (Input.GetButton("Fire2")) //Clear drawing and vector field
        {
            if (!isDrawing)
            {
                ClearRenderer();
                ClearVectorField();
            }
        }
    }

    private void CalculateVectorFieldState()
    {
        float stepX = mapSizeX / cutNumber;
        float stepY = mapSizeZ / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = stepX * i;
                float y = stepY * j;

                Vector2 lineOneA = new Vector2(x, y);
                Vector2 lineOneB = new Vector2(-10, 10);

                Vector2 lineTwoA = patternVertices[patternVertices.Count - 1];

                int count = 0;
                foreach (Vector2 v in patternVertices)
                {
                    if (GeometryTools.LineSegmentsIntersect(lineOneA, lineOneB, lineTwoA, v)) count++;
                    lineTwoA = v;
                }
                if (count % 2 == 1) vectorFieldState[i, j] = true;
                else vectorFieldState[i, j] = false;
            }
        }
    }

    private void CalculateVectorField()
    {
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                if(vectorFieldState[i,j])
                {
                    vectorField[i, j] = Vector2.zero;
                } else
                {
                    if(NeighboringCaseIncludedInTheForm(i, j))
                    {
                        vectorField[i, j]=GetNeighboringInTheFormDirection(i, j);
                    } else
                    {
                        vectorField[i, j] = Vector2.zero; //Will be updated during the smoothing phase
                    }
                }
            }
        }
    }

    private void SmoothingVectorField()
    {
        int iterationNb = cutNumber;
        for(int k=0; k<iterationNb; k++)
        {
            for (int i = 0; i < cutNumber; i++)
            {
                for (int j = 0; j < cutNumber; j++)
                {
                    if (vectorFieldState[i, j])
                    {
                        //Do nothing
                    }
                    else
                    {
                        if (NeighboringCaseIncludedInTheForm(i, j))
                        {
                            //Do nothing
                        }
                        else
                        {
                            vectorField[i, j] = GetNeighboringMeanForce(i, j); 
                        }
                    }
                }
            }
        }
    }


    private void ClearVectorField()
    {
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                vectorFieldState[i, j] = false;
                vectorField[i, j] = Vector2.zero;
            }
        }
    }

    private bool NeighboringCaseIncludedInTheForm(int x, int y)
    {
        bool res = false;
        for(int i=-1; i<2; i++)
        {
            for(int j=-1; j<2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else if (vectorFieldState[x2, y2]) res = true;
            }
        }
        return res;
    }

    private Vector2 GetNeighboringInTheFormDirection(int x, int y)
    {
        Vector2 res = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else if (vectorFieldState[x2, y2]) res += new Vector2(i, j);
            }
        }
        return res.normalized;
    }

    private Vector2 GetNeighboringMeanForce(int x, int y)
    {
        Vector2 res = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int x2 = x + i;
                int y2 = y + j;

                if (x2 < 0 || x2 >= cutNumber || y2 < 0 || y2 >= cutNumber) break;
                else res += vectorField[x2, y2];
            }
        }
        return res.normalized;
    }


    private void DrawVectorFieldState()
    {
        float stepX = mapSizeX / cutNumber;
        float stepY = mapSizeZ / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = stepX * i;
                float y = stepY * j;

                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                if(vectorFieldState[i,j]) lineRenderer.material.color = Color.red;
                else lineRenderer.material.color = Color.blue;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 4;
                lineRenderer.useWorldSpace = true;

                lineRenderer.SetPosition(0, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, new Vector3(x+0.01f, 0.1f, y + 0.01f)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(2, new Vector3(x, 0.1f, y + 0.01f)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(3, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line

                patternRenderer.Add(lineRenderer);

            }
        }
    }

    private void DrawVectorField()
    {
        float stepX = mapSizeX / cutNumber;
        float stepY = mapSizeZ / cutNumber;
        for (int i = 0; i < cutNumber; i++)
        {
            for (int j = 0; j < cutNumber; j++)
            {
                float x = stepX * i;
                float y = stepY * j;

                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                if (vectorFieldState[i, j]) lineRenderer.material.color = Color.red;
                else lineRenderer.material.color = Color.blue;
                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;

                lineRenderer.SetPosition(0, new Vector3(x, 0.1f, y)); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, new Vector3(x + vectorField[i, j].x / 20.0f, 0.1f, y + vectorField[i, j].y / 20.0f)); ; //x,y and z position of the starting point of the line
                patternRenderer.Add(lineRenderer);
            }
        }
    }

    private void DrawPattern()
    {
        LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
        lineRenderer.material.color = Color.black;
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = patternVertices.Count+1;
        lineRenderer.useWorldSpace = true;

        int count = 0;
        foreach (Vector2 v in patternVertices)
        {
            lineRenderer.SetPosition(count, new Vector3(v.x, 0.1f, v.y)); //x,y and z position of the starting point of the line
            count++;
        }
        lineRenderer.SetPosition(count, new Vector3(patternVertices[0].x, 0.1f, patternVertices[0].y)); //x,y and z position of the starting point of the line
        patternRenderer.Add(lineRenderer);
    }


    private void ClearRenderer()
    {
        foreach(LineRenderer l in patternRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        patternRenderer.Clear();
    }


    public Vector2 GetEnvironmentalForce(Vector2 position)
    {
        float stepX = mapSizeX / cutNumber;
        float stepY = mapSizeZ / cutNumber;
        int x = (int)(position.x / stepX);
        int y = (int)(position.y / stepY);

        return vectorField[x, y];
    }
}
