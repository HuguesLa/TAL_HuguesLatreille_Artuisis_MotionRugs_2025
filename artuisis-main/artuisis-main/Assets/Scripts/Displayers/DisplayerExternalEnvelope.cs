using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerExternalEnvelope : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;
    #endregion

    #region Private fields
    private List<LineRenderer> visualRenderer;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        visualRenderer = new List<LineRenderer>();

        this.material.SetFloat("_Mode", 2);
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<List<Vector3>> convexHuls = SwarmTools.GetConvexHul(swarmData);

        foreach(List<Vector3> pile in convexHuls)
        {

                //For creating line renderer object
                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;

                lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = pile.Count+1;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = material;
                //lineRenderer.material.SetFloat("_Mode", 2);
                lineRenderer.material.color = Color.red;

                for (int i = 0; i < pile.Count; i++)
                {
                    //For drawing line in the world space, provide the x,y,z values
                    lineRenderer.SetPosition(i, pile[i]); //x,y and z position of the starting point of the line
                }
                lineRenderer.SetPosition(pile.Count, pile[0]);
                lineRenderer.transform.parent = this.transform;
                visualRenderer.Add(lineRenderer);
            
        }
    }

    public override void ClearVisual()
    {
        foreach (LineRenderer l in visualRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        visualRenderer.Clear();
    }

    #endregion

    
}
