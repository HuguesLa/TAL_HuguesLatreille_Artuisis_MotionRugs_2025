using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Text;

public class ResultsFormatConverter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ExperimentationResult res = LoadResultsDat("C:/Users/hmaym/OneDrive/Bureau/participant_pilote_3.dat");
        StringBuilder sb = new StringBuilder();
        //Header
        string line = "Filename;Framenumber;Result\r";
        sb.Append(line);


        foreach(ClipResult cr in res.results)
        {
            line = cr.filename + ";" + cr.frameNumber + ";" + cr.fracture + "\r";
            sb.Append(line);
        }

        File.AppendAllText(Application.dataPath + "/Results/" + "participant_pilote_3.csv", sb.ToString());
        sb.Clear();
        Debug.Log("Finished");


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ExperimentationResult LoadResultsDat(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            ExperimentationResult results = null;
            try
            {
                results = (ExperimentationResult)bf.Deserialize(file);
            }
            catch (Exception)
            {
                return null;
            }

            file.Close();
            return results;
        }
        else
        {
            return null;
        }
    }
}
