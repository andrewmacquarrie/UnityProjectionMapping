using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets;
using System.Xml.Serialization;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PointsRecordReplay : MonoBehaviour {
    public CorrespondenceAcquisition pointsHolder;

	void Start () {
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RecordCurrentPoints();
            Debug.Log("Recording complete");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadSavedPoints();
            Debug.Log("Loading complete");
        }
	}

    private void LoadSavedPoints()
    {
        string fileName;
        #if UNITY_EDITOR
            fileName = EditorUtility.OpenFilePanel("Choose the file path", "", "xml");
        #endif
        #if UNITY_EDITOR == false
                fileName = "recording.xml";
        #endif
        var recording = Recording.LoadFromFile(fileName);
        pointsHolder.ReplayRecordedPoints(recording.worldPointsV3, recording.ImagePointsV3((double)Screen.width, (double)Screen.height));
    }

    private void RecordCurrentPoints()
    {
        string fileName;
        #if UNITY_EDITOR
            fileName = EditorUtility.SaveFilePanel("Choose the file path", "", "pointsRecording", "xml");
        #endif
        #if UNITY_EDITOR == false
            fileName = "recording.xml";
        #endif
        SaveToFile(pointsHolder.GetImagePoints(), pointsHolder.GetWorldPoints(), fileName);
    }

    private static void SaveToFile(List<Vector3> imagePoints, List<Vector3> worldPoints, string fileName)
    {
        var recording = new Recording(imagePoints, worldPoints, (double)Screen.width, (double)Screen.height);
        recording.SaveToFile(fileName);
    }
}
