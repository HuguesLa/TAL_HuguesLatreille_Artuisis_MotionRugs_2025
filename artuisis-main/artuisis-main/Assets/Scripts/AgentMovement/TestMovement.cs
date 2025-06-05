using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TestMovement : MonoBehaviour
{

    public GameObject monaVisual;

    [Range(-1f,1f)]
    public float leftIntensity = 0.0f;
    [Range(-1f, 1f)]
    public float rightIntensity = 0.0f;
    public float maxSpeed = 0.1f;

    private AgentData agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = new AgentData(Vector3.zero, Vector3.forward);
        UpdateDirectionAndPosition();

    }

    // Update is called once per frame
    void Update()
    {
        Tuple<Vector3,Vector3> temp = SimulatedMonaMovement.Move(agent.GetPosition(), agent.GetDirection(), leftIntensity, rightIntensity, Time.deltaTime, maxSpeed);

        agent.SetPosition(temp.Item1);
        agent.SetDirection(temp.Item2);
        UpdateDirectionAndPosition();
    }


    private void UpdateDirectionAndPosition()
    {

        monaVisual.transform.localPosition = agent.GetPosition();

        float agentDirection_YAxis = 180 - (Mathf.Acos(agent.GetDirection().normalized.x) * 180.0f / Mathf.PI);
        if (agent.GetDirection().z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
        monaVisual.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);
    }

}
