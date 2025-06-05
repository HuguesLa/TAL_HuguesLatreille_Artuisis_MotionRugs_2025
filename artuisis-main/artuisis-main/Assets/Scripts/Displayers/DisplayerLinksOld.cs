using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerLinksOld : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;
    #endregion

    #region Private fields
    private List<LineRenderer> linksRenderer;

    //Gradient parameters
    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;
    #endregion

    #region Methods - MonoBehaviour callbacks
    private void Start()
    {
        linksRenderer = new List<LineRenderer>();


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.black;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.black;
        colorKey[1].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.5f;
        alphaKey[1].alpha = 0.05f;
        alphaKey[1].time = 1.0f;


        gradient.SetKeys(colorKey, alphaKey);
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<Tuple<AgentData, AgentData>> links = SwarmTools.GetLinksList(swarmData);

        float fovSize = swarmData.GetParameters().GetFieldOfViewSize();

        foreach(Tuple<AgentData,AgentData> l in links)
        {
            float distOnMaxDistance = Vector3.Distance(l.Item1.GetPosition(), l.Item2.GetPosition()) / fovSize;
            Color lineColor = gradient.Evaluate(distOnMaxDistance);


            //For creating line renderer object
            LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = material;
            //lineRenderer.material.SetFloat("_Mode", 2);
            lineRenderer.material.color = lineColor;

            //For drawing line in the world space, provide the x,y,z values
            lineRenderer.SetPosition(0, l.Item1.GetPosition()); //x,y and z position of the starting point of the line
            lineRenderer.SetPosition(1, l.Item2.GetPosition()); //x,y and z position of the end point of the line


            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            lineRenderer.transform.parent = this.transform;

            linksRenderer.Add(lineRenderer);
        }
    }

    public override void ClearVisual()
    {
        ClearLinksRenderer();
    }
    #endregion

    #region Methods - Other methods
    private void ClearLinksRenderer()
    {
        foreach (LineRenderer l in linksRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        linksRenderer.Clear();
    }
    #endregion
}
