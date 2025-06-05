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

public class ExperimentationRightInteractionPlayer : MonoBehaviour
{
    #region Serialize fields
    [SerializeField]
    private ClipPlayer clipPlayer;

    [SerializeField]
    private Slider progressionSlider;

    [SerializeField]
    private GameObject answerMenu;

    [SerializeField]
    private Button button1;    
    
    [SerializeField]
    private Button button2;

    [SerializeField]
    private GameObject startingMenu;

    [SerializeField]
    private GameObject finishMenu;

    [SerializeField]
    private Displayer defaultVisualisation;

    [SerializeField]
    private List<Displayer> testedVisualisation;

    [SerializeField]
    private bool allowRotation = true;

    [SerializeField]
    private GameObject displayers;
    #endregion

    #region Private fields
    //--Clip loading--//
    string filePath = "/Clips/"; //The folder containing clip files

    string[] filePaths;

    List<Tuple<string, string, string>> dataTask2;

    Thread backgroundThread;

    List<Tuple<int, int,int>> experimentalConditions; //Tuple<idClip, idVisu> = an experimental condition, with a clip and the associated visualisation

    List<int> loadOrder;

    private SwarmClip[] clips;


    //--Clip player--//

    private int currentCondition = 0;

    //--Results--//
    private Exp2AnticipationResult experimentationResult = new Exp2AnticipationResult();

    string resultFilePathCSV = "";
    string resultFilePathDat = "";

    private bool answered = false;

    private bool resultSaved = false;

    float participantHeight = 0.0f;

    float passedTime = 0.0f;

    StringBuilder sb = new StringBuilder();

    private bool reverseAnswer = false;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Disable gameObject
        answerMenu.SetActive(false); //Answering menu
        finishMenu.SetActive(false); //Answering menu

        //Enable gameObject
        startingMenu.SetActive(true);

        //Get the path of the clip files
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        //Get the files path from the clip folder
        filePaths = Directory.GetFiles(filePath, "*.dat",
                                         SearchOption.TopDirectoryOnly);

        ReadCSVFile();

        List<string> fileToKeep = new List<string>();
        foreach(string f in filePaths)
        {
            String s = Experiment2Tools.GetFileName(f);

            bool usedFile = false;
            foreach(Tuple<string,string, string> data in dataTask2)
            {
                if(String.Equals(data.Item1,s))
                {
                    usedFile = true;
                }
            }

            if (usedFile)fileToKeep.Add(f);
        }

        filePaths = fileToKeep.ToArray();


        //Prepare the name of the result files
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss"); 
        string resultFilename = "/" + "resultT2_" + date; //it uses the date to obtain a unique name
        string resultFolderPath = Application.dataPath + "/Results";
        resultFilePathDat = resultFolderPath + resultFilename + ".dat";
        resultFilePathCSV = resultFolderPath + resultFilename + ".csv";

        //Check if the result folder exists. If not, create one
        if (!Directory.Exists(resultFolderPath))
        {
            Directory.CreateDirectory(resultFolderPath);
        }

        //Prepare csv result file
        string line = "Filename,Visualisation,Rotation,Result,Height,AnswerTime\r";
        sb.Append(line);

        if (clipPlayer == null)
        {
            Debug.LogError("There is no ClipPlayer in the scene.", this);
        }


        experimentalConditions = Experiment2Tools.CreateExperimentalConditions(filePaths.Length, testedVisualisation.Count + 1);


        //Obtenir l'ordre de chargement des clips
        this.loadOrder = new List<int>();
        foreach (Tuple<int,int,int> cond in experimentalConditions)
        {
            int clipId = cond.Item1;
            if(!loadOrder.Contains(clipId))
            {
                loadOrder.Add(clipId);
            }
        }


        clips = new SwarmClip[filePaths.Length];


        //Load the first clip before continue
        bool succes = LoadFirstClips();
        
        //Prepare the thread that will load the other clips
        backgroundThread = new Thread(new ThreadStart(LoadOtherClips));

        // Start thread loading the other clips
        backgroundThread.Start();

 
        if (succes)
        {
            currentCondition = -1;
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
        
        if (clipPlayer.IsClipFinished() && currentCondition != -1 && !resultSaved) //If the current clip ended
        {
                //Display answering menu
                progressionSlider.value = (float)(currentCondition + 1) / (float)experimentalConditions.Count;
                answerMenu.SetActive(true);
                passedTime += Time.deltaTime;
                clipPlayer.AllowDisplay(false);
        }
        else
        {
            answerMenu.SetActive(false);
        }
    }

    public void StartExperimentation()
    {
        if (currentCondition == -1) //If no clip was display
        {
            //Disable starting menu
            startingMenu.SetActive(false);

            participantHeight = Camera.main.transform.position.y;

            //Launch the first clip
            NextClip();
        } else
        {
            Debug.LogError("Can't start the experiment.",this);
        }
    }

    private void NextClip()
    {

        if (currentCondition >= experimentalConditions.Count - 1)
        {
            //Display finish menu
            finishMenu.SetActive(true);
            SaveResult();
    
            Debug.Log("Experimentation finished");
        }
        else
        {
            currentCondition++;

            clipPlayer.AllowDisplay(true);
            //Changing display
            List<Displayer> newDisplay = new List<Displayer>();
            newDisplay.Add(defaultVisualisation);
            if (experimentalConditions[currentCondition].Item2 != 0)
            {
                newDisplay.Add(testedVisualisation[experimentalConditions[currentCondition].Item2 - 1]);
            }
            clipPlayer.SetUsedDisplayers(newDisplay);

            string s = Experiment2Tools.GetFileName(filePaths[experimentalConditions[currentCondition].Item1]);
            Tuple<string, string, string> dataAnswer = new Tuple<string, string, string>("","","");
            foreach (Tuple<string,string,string> data in dataTask2)
            {
                if (String.Equals(data.Item1, s))
                {
                    dataAnswer = data;
                    break;
                }
            }
            Debug.Log(dataAnswer.Item1 + dataAnswer.Item2 + dataAnswer.Item3);
            int val = (int)UnityEngine.Random.Range(1, 100);
            if (val % 2 == 0)
                reverseAnswer = false;
            else
                reverseAnswer = true;
                
            if(reverseAnswer)
            {
                button1.GetComponentInChildren<Text>().text = dataAnswer.Item3.ToString();
                button2.GetComponentInChildren<Text>().text = dataAnswer.Item2.ToString();
            }
            else
            {
                button1.GetComponentInChildren<Text>().text = dataAnswer.Item2.ToString();
                button2.GetComponentInChildren<Text>().text = dataAnswer.Item3.ToString();
            }

            //Mirror clip
            SwarmClip c = clips[experimentalConditions[currentCondition].Item1];
            if (experimentalConditions[currentCondition].Item3 ==1)
            {
                c = ClipTools.MirrorClip(c,true,false);
            }

           //Changing clip
            clipPlayer.SetClip(c);
            Debug.Log("Next clip : " + (experimentalConditions[currentCondition].Item1) + " and visu number : "+ (experimentalConditions[currentCondition].Item2));

            /*
            if (allowRotation)
            {
                //Experiment2Tools.RotateDisplayers(displayers, experimentalConditions[currentCondition].Item3);
            }
                    
            else
            {
                //Experiment2Tools.RotateDisplayers(displayers, 0);
            }*/
                    

            clipPlayer.Invoke("Play",1.0f);
            answered = false;
            passedTime = 0.0f;
        }
        
    }


    //Choice = true   => fracture
    public void GiveAnswer(bool choice)
    {
        if (!answered && clipPlayer.IsClipFinished())
        {
            //Get file name from file path
            string s = Experiment2Tools.GetFileName(filePaths[experimentalConditions[currentCondition].Item1]);
            int v = experimentalConditions[currentCondition].Item2;
            if (reverseAnswer) choice = !choice;
            int r = 0;
            if (allowRotation)
            {
                r = experimentalConditions[currentCondition].Item3;
            }
            //r = Experiment2Tools.GetCorrespondingRotation(r);

            Exp2AnticipationAnswer res = new Exp2AnticipationAnswer(s,v,r, choice, Camera.main.transform.position.y,passedTime);
            Debug.Log("Time " + passedTime);
            Debug.Log(res.filename + "    " + res.fracture);
            experimentationResult.AddClipResult(res);
            passedTime = 0.0f;
        }
        answered = true;
        NextClip();
    }


    public void SaveResult()
    {
        //Save result : format .dat
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(resultFilePathDat, FileMode.OpenOrCreate);
        bf.Serialize(file, experimentationResult);
        file.Close();


        //Save result : format .csv
        foreach (Exp2AnticipationAnswer cr in experimentationResult.results)
        {
            string line;
            line = cr.filename + "," + cr.visualisation + "," + cr.rotation + "," + cr.fracture + "," + cr.height.ToString().Replace(",", ".") + "," + cr.answerTime.ToString().Replace(",", ".") +"\r";
            sb.Append(line);
        }

        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        resultSaved = true;
        Debug.Log("Results saved.");
    }

    #region Methods - Load clips

    public bool LoadFirstClips()
    {

        string s = filePaths[loadOrder[0]];

        //Loading clip from full file path
        SwarmClip clip = ClipTools.LoadClip(s);

        if (clip != null)
        {
            clips[loadOrder[0]]= clip; //Add the loaded clip to the list
            Debug.Log("Clip " + s + " Loaded.");
            return true;
        }
        else
        {
            Debug.LogError("Clip can't be load from " + s, this);
            return false;
        }


    }

    public void LoadOtherClips()
    {
        //Load all clip
        for (int i = 1; i < filePaths.Length; i++)
        {
            //Concatenation of file path and file name
            string s = this.filePaths[loadOrder[i]];

            //Loading clip from full file path
            SwarmClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips[loadOrder[i]] = clip; //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);
        }
    }

    #endregion

    private void ReadCSVFile()
    {
        dataTask2 = new List<Tuple<string, string, string>>();
        StreamReader strReader = new StreamReader(Application.dataPath + "/Clips/DataTask2.csv");
        bool endOfFile = false;
        while(!endOfFile)
        {
            string data_string = strReader.ReadLine();
            if(data_string == null)
            {
                endOfFile = true;
                break;
            }

            
            var data_values = data_string.Split(',');
            Tuple<String, String, String> data = new Tuple<string, string, string>(data_values[0], data_values[1], data_values[2]);
            dataTask2.Add(data);
            /*for (int i=0; i< data_values.Length; i++)
            {
                Debug.Log("Value : " + i.ToString() + " " + data_values[i].ToString());
            }*/
        }
    }

}
