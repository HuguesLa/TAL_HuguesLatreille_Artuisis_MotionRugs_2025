using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ClipTools
{
    #region Methods - Load and Save

    /// <summary>
    /// This method load a clip from a specific file format (.dat), containing a <see cref="SwarmClip"/> instance.
    /// </summary>
    /// <param name="filePath"> A <see cref="string"/> value corresponding to the absolute path of the file to load</param>
    /// <returns>
    /// A <see cref="SwarmClip"/> instance from the file, null otherwise. 
    /// </returns>
    public static SwarmClip LoadClip(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            SwarmClip clip = null;
            try
            {
                clip = (SwarmClip)bf.Deserialize(file);
            }
            catch (Exception)
            {
                return null;
            }

            file.Close();
            return clip;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// This method save a <see cref="SwarmClip"/> into a .dat file.
    /// </summary>
    /// <param name="clip"> The <see cref="SwarmClip"/> to save.</param>
    /// <param name="filePath"> The absolute path of the file that will contain the clip.</param>
    public static void SaveClip(SwarmClip clip, string filePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
        bf.Serialize(file, clip);
        file.Close();
    }
    #endregion

    #region Methods - Clip transformations
    public static SwarmClip MirrorClip(SwarmClip clip, bool mirrorXvalue, bool mirrorZvalue)
    {
        SwarmClip clone = clip.Clone();
        List<SwarmData> frames = clone.GetFrames();

        Vector3 transformation = new  Vector3(1.0f, 1.0f, 1.0f);

        if (mirrorXvalue) transformation.x = -1.0f;
        if (mirrorZvalue) transformation.z = -1.0f;

        foreach(SwarmData f in frames)
        {
            List<AgentData> agents = f.GetAgentsData();
            float xMap = f.GetParameters().GetMapSizeX();
            float zMap = f.GetParameters().GetMapSizeZ();

            foreach(AgentData a in agents)
            {
                //Mirror position
                Vector3 p = a.GetPosition();
                p.x = p.x * transformation.x;
                p.z = p.z * transformation.z;

                if (mirrorXvalue) p.x += xMap;
                if (mirrorZvalue) p.z += zMap;

                a.SetPosition(p);

                //Mirror speed
                Vector3 s = a.GetSpeed();
                s.x = s.x * transformation.x;
                s.z = s.z * transformation.z;
                a.SetSpeed(s);

                //Mirror acceleration
                Vector3 acc = a.GetAcceleration();
                acc.x = acc.x * transformation.x;
                acc.z = acc.z * transformation.z;
                a.SetAcceleration(acc);

                //Mirror direction
                Vector3 dir = a.GetDirection();
                dir.x = dir.x * transformation.x;
                dir.z = dir.z * transformation.z;
                a.SetDirection(dir);

                //Mirror forces
                List<Vector3> forces = a.GetForces();
                List<Vector3> mirrorForces = new List<Vector3>();
                foreach(Vector3 fo in forces)
                {
                    Vector3 temp = new Vector3(fo.x, fo.y, fo.z);
                    temp.x = temp.x * transformation.x;
                    temp.z = temp.z * transformation.z;
                    mirrorForces.Add(temp);
                }
                a.SetForces(mirrorForces);
            }
        }

        return clone;
    }
    #endregion
}
