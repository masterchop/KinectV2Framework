using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

    public int nextLevel;

	// Use this for initialization
	void Start () {
        nextLevel = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUI.Button(new Rect(100, 100, 100, 30), "Next Scene"))
        {
            DontDestroyOnLoad(transform.gameObject);
            Application.LoadLevel(nextLevel++);
        }
    }
}
