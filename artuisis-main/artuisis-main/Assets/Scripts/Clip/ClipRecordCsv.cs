using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ClipRecordCsv : MonoBehaviour
{
    #region Serialized fields
    [SerializeField] private SwarmManager swarmManager;
    [SerializeField] private Button recordButton; // Bouton pour démarrer/arrêter l'enregistrement
    [SerializeField] private Text statusText; // Texte d'état optionnel pour l'interface
    
    [Header("Configuration des chemins")]
    [SerializeField] private string recordedClipsPath = "/RecordedClips/test/";
    [SerializeField] private string csvOutputPath = "/Results/";
    
    [Header("Options d'enregistrement")]
    [SerializeField] private bool saveIntermediateDatFile = true;
    [SerializeField] private bool saveMetadataFile = true;
    
    [SerializeField] 
    [Range(0, 50)] 
    private int fps = 20; 
    #endregion

    #region Private fields
    private bool recording = false;
    private bool justStoppedRecording = false;
    private List<SwarmData> frames = new List<SwarmData>();
    private float lastRecordTime = 0f;
    private string currentClipName = "";
    
    // Dictionnaire pour stocker les directions/accélérations précédentes de chaque agent
    private Dictionary<int, Vector3> previousDirections = new Dictionary<int, Vector3>();
    private Dictionary<int, Vector3> previousAccelerations = new Dictionary<int, Vector3>();
    
    // Pour l'enregistrement CSV en temps réel
    private StringBuilder csvBuilder = new StringBuilder();
    private string currentCsvPath = "";
    private string currentMetadataPath = "";
    #endregion

    #region MonoBehaviour callbacks
    void Start(){
        if (swarmManager == null) Debug.LogError("SwarmManager is missing.", this);
        
        if (recordButton != null){
            recordButton.onClick.AddListener(ChangeRecordState);
        }
        
        if (statusText != null){
            statusText.text = "Prêt à enregistrer";
        }
        
        EnsureDirectoriesExist();
    }

   
    void FixedUpdate(){
        float recordingUpdateRate;
        recordingUpdateRate = 1/(float)fps;
        if (recording){
            // Vérifier si assez de temps s'est écoulé depuis le dernier enregistrement
            if (Time.time - lastRecordTime >= recordingUpdateRate){
                RecordFrame();
                Debug.Log($"Enregistrement frame #{frames.Count}");
                lastRecordTime = Time.time;
            }
            justStoppedRecording = false;
        }
        else if (justStoppedRecording){
            FinishRecording();
            frames.Clear();
            justStoppedRecording = false;
        }
    }
    #endregion

    #region Méthodes d'enregistrement
    private void EnsureDirectoriesExist(){
        string datPath = Application.dataPath + recordedClipsPath;
        string csvPath = Application.dataPath + csvOutputPath;
        
        if (!Directory.Exists(datPath)){
            Directory.CreateDirectory(datPath);
            Debug.Log("Dossier créé: " + datPath);
        }
        
        if (!Directory.Exists(csvPath)){
            Directory.CreateDirectory(csvPath);
            Debug.Log("Dossier créé: " + csvPath);
        }
    }

    private void RecordFrame(){
        SwarmData currentFrame = swarmManager.CloneFrame();
        frames.Add(currentFrame);
        AppendFrameToCSV(currentFrame, frames.Count - 1);
        
        if (frames.Count % 50 == 0) // Log tous les 50 frames pour ne pas surcharger la console
        {
            Debug.Log($"Frames enregistrées: {frames.Count}");
        }
    }

    /// <summary>
    /// Démarre ou arrête l'enregistrement
    /// </summary>
    public void ChangeRecordState(){
        recording = !recording;
        
        if (recording){
            StartNewRecording();
        }
        else{
            justStoppedRecording = true;
        }
        
        if (statusText != null){
            statusText.text = recording ? "Enregistrement en cours..." : "Enregistrement terminé";
        }
    }

    private void StartNewRecording(){
        // Réinitialiser
        frames.Clear();
        previousDirections.Clear();
        previousAccelerations.Clear();
        csvBuilder.Clear();
        
        // Créer un nom de fichier basé sur la date et l'heure actuelles
        string date = System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
        currentClipName = "clip_" + date;
        
        // Préparer les fichiers CSV
        currentCsvPath = Application.dataPath + csvOutputPath + currentClipName + "_data.csv";
        currentMetadataPath = Application.dataPath + csvOutputPath + currentClipName + "_metadata.csv";
        
        // Ajouter l'en-tête du CSV de données
        csvBuilder.Append("frame,id,x,y,DirectionChange,AccelerationChange,distance_centroid,bouncesOffWall,cohesion,separation,alignement,DominantParameter\n");
        File.WriteAllText(currentCsvPath, csvBuilder.ToString());
        
        Debug.Log("Démarrage d'un nouvel enregistrement: " + currentClipName);
        Debug.Log("CSV en cours de création: " + currentCsvPath);
        lastRecordTime = Time.time;
    }

    private void FinishRecording(){
        if (frames.Count == 0){
            Debug.Log("Aucune frame enregistrée.");
            return;
        }
        
        Debug.Log($"Enregistrement terminé: {frames.Count} frames.");
        Debug.Log("Clip Saved");
        
        // Sauvegarder le fichier .dat si nécessaire
        if (saveIntermediateDatFile){
            SwarmClip clip = new SwarmClip(frames);
            string datFilePath = Application.dataPath + recordedClipsPath + currentClipName + ".dat";
            ClipTools.SaveClip(clip, datFilePath);
            Debug.Log("Fichier .dat sauvegardé: " + datFilePath);
        }
        
        // Sauvegarder les métadonnées si nécessaire (bonus)
        if (saveMetadataFile){
            SaveClipMetadata();
            Debug.Log($"Métadonnées: {currentMetadataPath}");
        }
        
        Debug.Log("Enregistrement complet !");
        Debug.Log($"Données CSV: {currentCsvPath}");
    }
    #endregion
    

    #region Méthodes CSV
    private void AppendFrameToCSV(SwarmData frame, int frameIndex){
        StringBuilder tmpBuilder = new StringBuilder();
        List<AgentData> agents = frame.GetAgentsData();
        Vector3 centroid = CalculateCentroid(agents);
        
        // Parcourir tous les agents de cette frame
        for (int agentIndex = 0; agentIndex < agents.Count; agentIndex++){
            AgentData agent = agents[agentIndex];
            
            Vector3 position = agent.GetPosition();
            Vector3 direction = agent.GetDirection();
            Vector3 acceleration = agent.GetAcceleration();
            List<Vector3> listForces = agent.GetForces();
            
            // Calculer les magnitudes pour l'accélération et les forces
            float bouncesOffWall = listForces[4].magnitude;
            float cohesion = listForces[5].magnitude;
            float separation = listForces[6].magnitude;
            float alignement = listForces[7].magnitude;
            
            // Calculer la distance au centre
            float distanceToCentroid = Vector3.Distance(position, centroid);
            
            // Calculer le changement de direction
            float directionChange = 0f;
            if (previousDirections.ContainsKey(agentIndex)){
                Vector3 prevDir = previousDirections[agentIndex];
                // Vérifier que les vecteurs ne sont pas zéro pour éviter les erreurs
                if (direction.sqrMagnitude > 0 && prevDir.sqrMagnitude > 0){
                    // Calculer l'angle en degrés entre les deux directions
                    if(Vector3.Angle(direction, prevDir) > 0.08 ){
                        directionChange = Vector3.Angle(direction, prevDir);
                    }else {
                        directionChange = 0.01f;
                    }
                }
            }
            // Mettre à jour la direction précédente pour la prochaine frame
            previousDirections[agentIndex] = direction;
            
            float accelerationAbs = 0f;
            if (previousAccelerations.ContainsKey(agentIndex)){
                Vector3 prevAccel = previousAccelerations[agentIndex];
    
                // calcule l'angle entre deux vecteurs d'accélération
                if (acceleration.sqrMagnitude > 0.001f && prevAccel.sqrMagnitude > 0.001f){
                    accelerationAbs = Vector3.Angle(acceleration, prevAccel);
                }
            }
            // Mettre à jour l'accélération précédente pour la prochaine frame
            previousAccelerations[agentIndex] = acceleration;
            

            // Calcul de la force dominante 
            int dominantParameterValue = 0; // 0 signifie aucune force dominante
            Vector3[] forceVectors = new Vector3[] {
                listForces[5], // cohesion
                listForces[6], // separation
                listForces[7], // alignement
                listForces[4]  // bouncesOffWall
            };
            float[] forceIntensities = new float[forceVectors.Length];
            for (int i = 0; i < forceVectors.Length; i++) {
                forceIntensities[i] = forceVectors[i].magnitude;
            }
            for (int i = 0; i < forceIntensities.Length; i++) {
                bool best = true;
                for (int j = 0; j < forceIntensities.Length; j++) {
                    if (i == j) continue;
                    if (forceIntensities[i] < forceIntensities[j]) best = false;
                }
                if (best) {
                    float angle = Vector3.Angle(forceVectors[i], acceleration);
                    if (angle < 90) {
                        dominantParameterValue = i + 1; // 1: cohesion, 2: separation, 3: alignement, 4: bouncesOffWall
                    }
                    break;
                }
            }

            //float cohesionByDistance = cohesion / distanceToCentroid;
            //float cohesionAlignmentRatio = cohesion / alignement;

            string line = string.Format(CultureInfo.InvariantCulture,
                "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n",
                frameIndex,
                agentIndex,
                position.x,
                position.z, // y étant nul on met z
                directionChange,
                accelerationAbs,
                distanceToCentroid,
                bouncesOffWall,
                cohesion,
                separation,
                alignement,
                dominantParameterValue);
            
            tmpBuilder.Append(line);
        }
        
        // Ajouter au fichier CSV existant 
        try{
            File.AppendAllText(currentCsvPath, tmpBuilder.ToString());
        }
        catch (System.Exception e){
            Debug.LogError($"Erreur lors de l'écriture dans le fichier CSV: {e.Message}");
        }
    }
    
    // Calcule le centroïde à partir des positions de tous les agents
    private Vector3 CalculateCentroid(List<AgentData> agents){
        if (agents.Count == 0){
            return Vector3.zero;
        }
            
        Vector3 sum = Vector3.zero;
        foreach (AgentData agent in agents){
            sum += agent.GetPosition();
        }
        
        return sum / agents.Count;
    }

    // Bonus pour test
    private void SaveClipMetadata(){
        if (frames.Count == 0) return;
        
        StringBuilder sb = new StringBuilder();
        
        string header = "frame,id,x,y,frictionX,frictionY,frictionZ,avoidCollisionX,avoidColiisionY,avoidCollisionZ\n";
        sb.Append(header);
        
        for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++){
            SwarmData frame = frames[frameIndex];
            List<AgentData> agents = frame.GetAgentsData();
            
            for (int agentIndex = 0; agentIndex < agents.Count; agentIndex++){
                AgentData agent = agents[agentIndex];
                
                Vector3 position = agent.GetPosition();
                Vector3 direction = agent.GetDirection();
                List<Vector3> listForces = agent.GetForces();
                
                Vector3 friction = listForces[2];
                Vector3 avoidCollision = listForces[3];
                
                string line = string.Format(CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}\n",
                    frameIndex,
                    agentIndex,
                    position.x,
                    position.z,
                    friction.x,
                    friction.y,
                    friction.z,
                    avoidCollision.x,
                    avoidCollision.y,
                    avoidCollision.z);
                
                sb.Append(line);
            }
        }
        
        try{
            File.WriteAllText(currentMetadataPath, sb.ToString());
            Debug.Log("Métadonnées sauvegardées: " + currentMetadataPath);
        }
        catch (System.Exception e){
            Debug.LogError($"Erreur lors de l'écriture des métadonnées: {e.Message}");
        }
    }
    #endregion
}