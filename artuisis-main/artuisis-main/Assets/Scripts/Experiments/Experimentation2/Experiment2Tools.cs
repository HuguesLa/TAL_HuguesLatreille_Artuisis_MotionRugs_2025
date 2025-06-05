using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment2Tools
{
    public static List<Tuple<int,int,int>> CreateExperimentalConditions(int nbClips, int nbVisualisation)
    {
        //Créer une liste d'identifiant qui font références aux clips qui vont être chargés
        List<Tuple<int, int,int>>[] tab = new List<Tuple<int, int,int>>[nbVisualisation]; //IdClip, IdVisu, IdRotation


        //Shuffle visu order
        int[] idVisu = new int[nbVisualisation];
      
        for(int i = 0; i<nbVisualisation; i++)
        {
            idVisu[i] = i;
        }

        var rnd = new System.Random();
        idVisu = idVisu.ToList<int>().OrderBy(item => rnd.Next()).ToArray<int>();


        int offSet = UnityEngine.Random.Range(0, nbVisualisation);
        for (int v = 0; v < nbVisualisation; v++)
        {
            List<Tuple<int, int,int>> listPart = new List<Tuple<int, int,int>>();
            for (int c = 0; c < nbClips; c++)
            {
                int vis = idVisu[(v + c) % (nbVisualisation)];
                //int rot = (v + c + offSet) % 2; //TO DO
                int rot = (v + offSet) % 2; //TO DO
                listPart.Add(new Tuple<int, int,int>(c, vis,rot));
            }
            tab[v] = listPart;
        }


        
        String disp = "";
        for (int i = 0; i < nbClips; i++)
        {
            for (int j = 0; j < nbVisualisation; j++)
            {
                disp += tab[j][i].Item3 + ";";
            }
            Debug.Log(disp);
            disp = "";
        }
        

        //Suffle within blocks
        for (int i = 0; i < nbVisualisation; i++)
        {
            tab[i] = tab[i].OrderBy(item => rnd.Next()).ToList<Tuple<int, int,int>>();
        }

        //Protection against too-close duplicates
        int dist = 4;
        for (int i = 0; i < nbVisualisation-1; i++)
        {
            for (int j = 0; j < dist; j++)
            {
                for (int k = 0; k < dist - j; k++)
                {
                    int posTab1 = tab[i].Count - 1 - j;
                    if (tab[i][posTab1].Item1 == tab[i + 1][k].Item1)
                    {
                        Tuple<int, int,int> temp = tab[i][posTab1];
                        tab[i].RemoveAt(posTab1);
                        tab[i].Insert(tab[i].Count / 2, temp);
                        j = -1;
                        break;
                    }
                }
            }
        }

        //Merge the independant lists
        List<Tuple<int, int,int>>  experimentalConditions = new List<Tuple<int, int,int>>();
        for (int i = 0; i < nbVisualisation; i++)
        {
            experimentalConditions.AddRange(tab[i]);
        }

        return experimentalConditions;
    }


    public static string GetFileName(string filePath)
    {
        //Get file name from file path
        string s = filePath;
        int pos = s.IndexOf("/");
        while (pos != -1)
        {
            s = s.Substring(pos + 1);
            pos = s.IndexOf("/");
        }
        return s;
    }


    public static void RotateDisplayers(GameObject g,int val)
    {
        switch(val)
        {
            case 0:
                g.transform.rotation = Quaternion.Euler( new Vector3(0.0f, 0.0f, 0.0f));
                g.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                break;            
            case 1:
                g.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 90.0f, 0.0f));
                g.transform.position = new Vector3(0.0f, 0.0f, 7f);
                break;           
            case 2:
                g.transform.rotation = Quaternion.Euler(new Vector3(0.0f, 180f, 0.0f));
                g.transform.position = new Vector3(7.0f, 0.0f, 7.0f);
                break;            
            case 3:
                g.transform.rotation = Quaternion.Euler(new Vector3(0.0f,270.0f, 0.0f));
                g.transform.position = new Vector3(7.0f, 0.0f, 0.0f);
                break;
            default:
                Debug.LogError("Valeur anormale");
                break;
        }
    }

    public static int GetCorrespondingRotation(int val)
    {
        switch (val)
        {
            case 0:
                return 0;
            case 1:
                return 90;
            case 2:
                return 180;
            case 3:
                return 270;
            default:
                Debug.LogError("Valeur anormale");
                return -1;
        }
    }
}
