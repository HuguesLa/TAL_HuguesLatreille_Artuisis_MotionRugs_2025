using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSlider : MonoBehaviour
{
    [SerializeField]
    private EditorParametersInterface m_Parameters;

    [SerializeField]
    private TMP_Text feedback;

    [SerializeField] 
    private Slider slider;

    [SerializeField]
    private Parameter controlledParameter;

    [SerializeField]
    private float minValue=0.0f;

    [SerializeField]
    private float maxValue=1.0f;

    private enum Parameter
    {
        Cohesion,
        Separation,
        Alignment,
        Friction,
        MaxSpeed,
        Random,
        FOV,
        BS

    }

    // Start is called before the first frame update
    void Start()
    {
        GetParameterValue();
    }

    // Update is called once per frame
    void Update()
    {
        GetParameterValue();
    }

    private void GetParameterValue()
    {
        float val = 0.0f;
        switch (controlledParameter)
        {
            case Parameter.Cohesion:
                val = m_Parameters.GetCohesionIntensity();
                break;

            case Parameter.Separation:
                val = m_Parameters.GetSeparationIntensity();
                break;
            case Parameter.Alignment:
                val = m_Parameters.GetAlignmentIntensity();
                break;
            case Parameter.FOV:
                val = m_Parameters.GetFieldOfViewSize();
                break;
            case Parameter.BS:
                val = m_Parameters.GetBlindSpotSize();
                break;
            case Parameter.Random:
                val = m_Parameters.GetRandomMovementIntensity();
                break;
            case Parameter.MaxSpeed:
                val = m_Parameters.GetMaxSpeed();
                break;
            case Parameter.Friction:
                val = m_Parameters.GetFrictionIntensity();
                break;

        }

        float range = maxValue - minValue;

        float value = (val - minValue) / range;

        slider.value = value;

        feedback.text = val.ToString("F2");
    }


    public void UpdateParameter()
    {
        float value = slider.value;
        float range = maxValue - minValue;

        value = (range * value) + minValue;
        switch (controlledParameter)
        {
            case Parameter.Cohesion:
                m_Parameters.SetCohesionIntensity(value);
                break;

            case Parameter.Separation:
                m_Parameters.SetSeparatonIntensity(value);
                break;

            case Parameter.Alignment:
                m_Parameters.SetAlignmentIntensity(value);
                break;
            case Parameter.FOV:
                m_Parameters.SetFieldOfViewSize(value);
                break;
            case Parameter.BS:
                m_Parameters.SetBlindSpotSize(value);
                break;
            case Parameter.Random:
                m_Parameters.SetRandomMovementIntensity(value);
                break;
            case Parameter.MaxSpeed:
                m_Parameters.SetMaxSpeed(value);
                break;
            case Parameter.Friction:
                m_Parameters.SetFrictionIntensity(value);
                break;

        }

        feedback.text = value.ToString("F2");
    }
}
