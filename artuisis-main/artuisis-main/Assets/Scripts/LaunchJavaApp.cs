using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;

public class LaunchJavaApp : MonoBehaviour
{
    [Tooltip("Chemin vers le fichier JAR de l'application Java")]
    public string javaAppPath = "H:/Documents/TAL/testMRjar/MRTest.jar";

    [Tooltip("Chemin vers java.exe (laissez vide pour utiliser la variable PATH)")]
    public string javaPath = "";
    
    [Tooltip("Référence au bouton (optionnel, peut être configuré automatiquement)")]
    public Button launchButton;

    void Start()
    {
        if (launchButton == null)
        {
            launchButton = GetComponent<Button>();
            
            if (launchButton == null && transform.childCount > 0)
            {
                launchButton = GetComponentInChildren<Button>();
            }
        }
        
        if (launchButton != null)
        {
            launchButton.onClick.AddListener(LaunchApplication);
            UnityEngine.Debug.Log("Bouton configuré automatiquement pour lancer l'application Java");
        }
    }
    
    
    public void LaunchApplication()
    {
        try
        {
            if (!File.Exists(javaAppPath))
            {
                UnityEngine.Debug.LogError("Le fichier JAR n'a pas été trouvé à l'emplacement spécifié: " + javaAppPath);
                return;
            }

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
        
            if (string.IsNullOrEmpty(javaPath))
            {
                startInfo.FileName = "java";
            }
            else
            {
                startInfo.FileName = javaPath;
            }
            
            startInfo.Arguments = "-jar \"" + javaAppPath + "\"";
            
            startInfo.UseShellExecute = true;
        
            process.StartInfo = startInfo;
            
            process.Start();
            
            UnityEngine.Debug.Log("Application Java lancée avec succès!");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Erreur lors du lancement de l'application Java: " + e.Message);
        }
    }
}