using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownDisplayers : MonoBehaviour
{
    [SerializeField]
    private List<Displayer> displayerOptions;

    [SerializeField]
    private List<string> correspondingOptionName;

    [SerializeField]
    private TMP_Dropdown dropdown;


    [SerializeField]
    private SwarmManager manager;

    private int lastVal;


    // Start is called before the first frame update
    void Start()
    {
        CheckOption();

        List<string> list = new List<string>(correspondingOptionName);
        list.Add("None");
            
        dropdown.options.Clear();
        foreach (string option in list)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }
        dropdown.value = list.Count-1;
        dropdown.RefreshShownValue();

        lastVal = list.Count - 1;

        foreach(Displayer d in displayerOptions)
        {
            manager.RemoveUsedDisplayer(d);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckOption();
    }

    public void UpdateDisplayer()
    {
        int val = dropdown.value;

        if (lastVal < displayerOptions.Count)
        {
            manager.RemoveUsedDisplayer(displayerOptions[lastVal]);
        }

        if (val < displayerOptions.Count) {
            manager.AddUsedDisplayer(displayerOptions[val]);
            lastVal = val;
            
        }
    }

    private void CheckOption()
    {
        if (displayerOptions.Count != correspondingOptionName.Count) { Debug.LogError("Check displayer option or name, it seems one is missing", this); }
    }
}
