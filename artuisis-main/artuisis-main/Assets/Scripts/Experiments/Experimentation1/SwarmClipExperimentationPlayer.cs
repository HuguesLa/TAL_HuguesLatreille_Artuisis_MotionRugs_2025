using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

[RequireComponent(typeof(ClipPlayer))]
public class SwarmClipExperimentationPlayer : MonoBehaviour
{
    #region Serialize fields
    [SerializeField]
    private ClipPlayer clipPlayer;

    [SerializeField]
    private GameObject nextClipMenu;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TMP_Text feedback;
    #endregion

    #region Private fields
    //string filePath = "C:/Users/hmaym/OneDrive/Bureau/UnityBuild/Clip/"; //The folder containing clip files
    string filePath = "/Clips/"; //The folder containing clip files


    string[] filePaths;


    private List<SwarmClip> clips = new List<SwarmClip>();

    private int currentClip = 0;

    private ExperimentationResult experimentationResult = new ExperimentationResult();

    string resultFilePathCSV = "";
    string resultFilePathDat = "";

    private bool answered = false;

    private bool resultSaved = false;

    Thread backgroundThread;

    StringBuilder sb = new StringBuilder();
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        //Clips
        filePaths = Directory.GetFiles(filePath, "*.dat",
                                         SearchOption.TopDirectoryOnly);

        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string resultFilename = "/" + "resultXP_" + date;
        string resultFolderPath = Application.dataPath + "/Results";
        resultFilePathDat = resultFolderPath + resultFilename + ".dat";
        resultFilePathCSV = resultFolderPath + resultFilename + ".csv";

        if (!Directory.Exists(resultFolderPath))
        {
            Directory.CreateDirectory(resultFolderPath);
        }

        //Prepare csv result file
        string line = "Filename,Framenumber,Result\r";
        sb.Append(line);

        slider.gameObject.SetActive(false);

        if (clipPlayer == null)
        {
            Debug.LogError("There is no ClipPlayer in the scene.", this);
        }

        //Shuffle list
        var rnd = new System.Random();
        List<string> l = filePaths.ToList();
        l = l.OrderBy(item => rnd.Next()).ToList<string>();
        filePaths = l.ToArray();

        LoadFirstClips();
        backgroundThread = new Thread(new ThreadStart(LoadOtherClips));
        // Start thread loading the other clips
        backgroundThread.Start();

       
        if (clips.Count > 0)
        {
           currentClip = -1;
        }
    }

    private void OnDisable()
    {
        backgroundThread.Abort();
        Debug.Log("Thread is abort.");
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = clipPlayer.GetClipAdvancement();

        if (clipPlayer.IsClipFinished() || currentClip == -1)
        {
            //Display interclip menu and hide slider
            nextClipMenu.SetActive(true);
            slider.gameObject.SetActive(false);

            //Check if a response has been given
            if(!answered && !(currentClip ==-1))
            {
                GiveAnswer(false);
                answered = true;
            }

            //Check if the experimentation is ended to save the results
            if(currentClip >= clips.Count - 1 && !resultSaved)
            {
                nextClipMenu.GetComponentInChildren<TMP_Text>().text = "Finish";
                SaveResult();
                
            }
        }
        else
        {
            nextClipMenu.SetActive(false);
        }
    }

    public void NextClip()
    {
        if (clipPlayer.IsClipFinished())
        {
            if (currentClip >= clips.Count - 1)
            {
                Debug.Log("Experimentation finished");
            }
            else
            {
                currentClip++;
                clipPlayer.SetClip(clips[currentClip]);
                Debug.Log("Next clip" + (currentClip + 1));
                clipPlayer.Play();
                slider.gameObject.SetActive(true);
                answered = false;
                feedback.text = "";
            }
        }     
    }


    //Choice = true   => fracture
    public void GiveAnswer(bool choice)
    {   if(!answered)
        {
            feedback.text = choice ? "You answered \"Fracture\"" : "You answered \"No fracture\"";

            //Get file name from file path
            string s = filePaths[currentClip];
            int pos = s.IndexOf("/");
            while (pos != -1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            }

            ClipResult res = new ClipResult(s, choice, clipPlayer.GetFrameNumber());
            Debug.Log(res.filename + "    " + res.fracture + "   " + res.frameNumber);
            experimentationResult.AddClipResult(res);
        }
        answered = true;
    }


    public void SaveResult()
    {
        //Save result : format .dat
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(resultFilePathDat, FileMode.OpenOrCreate);
        bf.Serialize(file, experimentationResult);
        file.Close();


        //Save result : format .csv
        foreach (ClipResult cr in experimentationResult.results)
        {
            string line;
            line = cr.filename + "," + cr.frameNumber + "," + cr.fracture + "\r";
            sb.Append(line);
        }

        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        resultSaved = true;
        Debug.Log("Results saved.");
    }


    public void LoadFirstClips()
    {

        string s = filePaths[0];

        //Loading clip from full file path
        SwarmClip clip = ClipTools.LoadClip(s);

        if (clip != null)
        {
            clips.Add(clip); //Add the loaded clip to the list
            Debug.Log("Clip " + s + " Loaded.");
        }
        else
            Debug.LogError("Clip can't be load from " + s, this);

    }

    public void LoadOtherClips()
    {
        //Load all clip
        for (int i = 1; i< filePaths.Length; i++)
        {
            //Concatenation of file path and file name
            string s = this.filePaths[i];

            //Loading clip from full file path
            SwarmClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);
        }
    }

}
