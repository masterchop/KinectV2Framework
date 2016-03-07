using UnityEngine;
using System.Collections;

public class MyMattingControl : MonoBehaviour {

    private KinectRecorderPlayer saverPlayer;

	// Use this for initialization
	void Start () {
        saverPlayer = KinectRecorderPlayer.Instance;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void OnGUI()
    {
        if (saverPlayer)
        {
            if (GUI.Button(new Rect(100, 100, 100, 30), "Record"))
                saverPlayer.StartRecording();
            if (GUI.Button(new Rect(100, 140, 100, 30), "Play"))
                saverPlayer.StartPlaying();
            if (GUI.Button(new Rect(100, 180, 100, 30), "Stop"))
                saverPlayer.StopRecordingOrPlaying();
        }
    }
}
