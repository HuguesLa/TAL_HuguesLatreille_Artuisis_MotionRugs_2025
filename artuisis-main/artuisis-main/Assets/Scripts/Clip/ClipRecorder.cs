using System.Collections.Generic;
using UnityEngine;

public class ClipRecorder : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private SwarmManager swarmManager;
    #endregion

    #region Private fields
    private bool recording = false;

    private List<SwarmData> frames;
    #endregion

    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        if (swarmManager == null) Debug.LogError("AgentManager is missing.", this);

        frames = new List<SwarmData>();
    }

    // LateUpdate is called once per frame and after Update
    void FixedUpdate()
    {
        if (recording)
        {
            frames.Add(swarmManager.CloneFrame());
            Debug.Log("--frame");
        }
        else
        {
            if (frames.Count > 0)
            {
                Debug.Log("Clip Saved");
                SwarmClip clip = new SwarmClip(frames);
                //Create filename based on current date time
                string date = System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string filename = "/" + "clip_" + date + ".dat";
                //Save clip
                ClipTools.SaveClip(clip, Application.dataPath + "/RecordedClips/test" + filename);
                //Refresh recorder
                frames.Clear();
            }
        }
    }
    #endregion

    #region Methods

    /// <summary>
    /// Reverse the state of the recorder
    /// </summary>
    public void ChangeRecordState()
    {
        recording = !recording;
    }
    #endregion
}
