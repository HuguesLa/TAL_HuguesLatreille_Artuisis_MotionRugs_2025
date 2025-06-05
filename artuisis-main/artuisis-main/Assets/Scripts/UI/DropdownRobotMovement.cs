using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

public class DropdownRobotMovement : MonoBehaviour
{

    [SerializeField]
    private EditorParametersInterface m_Parameters;

    [SerializeField]
    private TMP_Dropdown dropdown;

   

    // Start is called before the first frame update
    void Start()
    {
        GetParameterValue();


        List<string> list = new List<string> { "Particle", "Mona robot" };
        dropdown.options.Clear();
        foreach (string option in list)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }
        dropdown.value = 0;
        dropdown.RefreshShownValue();

        m_Parameters.SetAgentMovement(MovementManager.AgentMovement.Particle);
    }

    // Update is called once per frame
    void Update()
    {
        GetParameterValue();
    }



    private void GetParameterValue()
    {

        MovementManager.AgentMovement val = m_Parameters.GetAgentMovenent();

        switch (val)
        {
            case MovementManager.AgentMovement.Particle:
                dropdown.value = 0;
                break;
            case MovementManager.AgentMovement.MonaRobot:
                dropdown.value = 1;
                break;
        }

        dropdown.RefreshShownValue();
    }

    public void UpdateAgentMovement()
    {
        int val = dropdown.value;

        switch(val)
        {
            case 0:
                m_Parameters.SetAgentMovement(MovementManager.AgentMovement.Particle);
                break;
            case 1:
                m_Parameters.SetAgentMovement(MovementManager.AgentMovement.MonaRobot);
                break;
        }
    }

}
