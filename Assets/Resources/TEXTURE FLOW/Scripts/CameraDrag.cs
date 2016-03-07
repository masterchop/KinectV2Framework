using UnityEngine;
using System.Collections;

public class CameraDrag : MonoBehaviour {
	
	Gallery gallery;
	Camera cam;
	Vector3 oldPos;
	Vector3 pos;
	[HideInInspector]
	public Vector3 deltaPos;
	//double zoomFactor = 0.2;

	
	// Use this for initialization
	void Start() {
		gallery = this.gameObject.GetComponent<Gallery>();
		cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update() {

		if(!GUI.changed)
		{
			if (Input.GetMouseButtonDown(0)) {
				Vector3 mousePos = Input.mousePosition;
				mousePos.z = 1;
				
				oldPos = cam.ViewportToWorldPoint(mousePos);
			}
			
			if (Input.GetMouseButton(0)) {
				Vector3 mousePos = Input.mousePosition;
				mousePos.z = 1;
				
				pos = cam.ViewportToWorldPoint(mousePos);


				if(gallery.viewMode == Gallery.ViewMode.horizontal)
				{
					deltaPos = new Vector3((oldPos.x - pos.x)/Screen.width, 0, oldPos.z - pos.z);
				}else if(gallery.viewMode == Gallery.ViewMode.vertical || gallery.viewMode == Gallery.ViewMode.grid){
					deltaPos = new Vector3(0, (oldPos.y - pos.y)/Screen.height, oldPos.z - pos.z);
				}

				//non-continuous movement
				//cam.transform.position += deltaPos;
			}
			
			if (Input.GetMouseButtonDown(0)) {
				oldPos = pos;
			}

			//place image gallery slightly more to the left when in Vertical mode only
			if(gallery.viewMode == Gallery.ViewMode.vertical)
			{
				cam.transform.position = new Vector3( 10, cam.transform.position.y, cam.transform.position.z);
			}

			//continuous movement

				cam.transform.position += deltaPos;

		}

	}
}