using UnityEngine;
using System.Collections;

//[ExecuteInEditMode]
public class Gallery : MonoBehaviour {

	public string title;
	public string extraTitle;
	public string targetFolder;
	public Font targetFont;
	public float spacing;
	public float rotationOffset;
	public float cameraDistance;
	public Color backgroundColor;
	public bool magnification = true;
	public bool reflections = true;
	public bool showTitle = true;
	public bool alignmentOptions = false;
	public Vector3 gridOrigin;

	Camera camera;
	Material background;
	CameraDrag cameraDrag;
	GameObject container;
	
	Object[] textures;
	GameObject[] gallery;
	GameObject[] reflectArray;
	GameObject[] names;
	
	GameObject focalElement = null;
	GameObject focalElementView = null;
	GameObject adjacentLeft = null;
	GameObject adjacentRight = null;
	GameObject previousAdjLeft = null;
	GameObject previousAdjRight = null;
	GameObject previouslyViewed = null;

	[HideInInspector]
	public Vector3 beginBound;
	[HideInInspector]
	public Vector3 endBound;

	GameObject TitleName;

	
	public enum DisplayNames{
		names,
		numbers,
		dontDisplay
	}

	public enum ViewMode{
		horizontal,
		vertical,
		grid
	}

	public enum CameraStart{
		Beginning,
		Middle,
		End
	}

	public CameraStart cameraStart = CameraStart.Beginning;
	public DisplayNames displayNames = DisplayNames.names;
	[HideInInspector]
	public ViewMode viewMode = ViewMode.horizontal;
	[HideInInspector]
	public int columns;
	private int initialColumns;



	// Use this for initialization
	void Start () {

		CreateTitle();

		//Gets all textures from the target folder
		textures = Resources.LoadAll(targetFolder);

		cameraDrag = this.GetComponent<CameraDrag>();

		//Creates an array of planes in the same size as the textures array
		GetTextures();

		//Adjusts textures spacing
		//SpaceOut();
		//Rename();
		ApplyChanges();

		ObjectsContainer();
	}

//	void DestroyContents()
//	{
		//Destroy(TitleName);
		//Destroy(container);
//	}


//	public void CreateNew () {
//
//		//DestroyContents();
//		CreateTitle();
//		
//		//Gets all textures from the target folder
//		textures = Resources.LoadAll(targetFolder);
//		
//		//Creates an array of planes in the same size as the textures array
//		GetTextures();
//		
//		//Adjusts textures spacing
//		//SpaceOut();
//		//Rename();
//		ApplyChanges();
//		
//		ObjectsContainer();
//	}
	
	// Update is called once per frame
	void Update () {
		//#if !UNITY_EDITOR
			if(magnification)
			{
				if(viewMode == ViewMode.grid){
					MouseZoom();
				}else{
					CameraZoom();
				}
			}
		ReactivatesCameraDrag();
		CameraMove();
		//#endif

	}

	void Background()
	{
		background = Resources.Load("TEXTURE FLOW/Materials/Skybox") as Material;
		RenderSettings.skybox = background;
		RenderSettings.skybox.SetColor("_Tint", backgroundColor);
	}
	

	//Parent all objects to a container
	void ObjectsContainer()
	{
		container = new GameObject();
		container.name = "container";
		container.transform.parent = this.gameObject.transform;

		for(int i = 0; i<= gallery.Length -1 ; i++)
		{
			gallery[i].transform.parent = container.transform;
		}
	}


	public void ApplyChanges()
	{
		SpaceOut();
		Rename();
		Reflections();
		ElementsRotation();
		Background();
	}

	void Reflections()
	{
		for(int i = 0; i<= reflectArray.Length -1 ; i++)
		{
			//applies only to horizontal mode
			if(reflections)
			{
				reflectArray[i].SetActive(true);
			}else{
				reflectArray[i].SetActive(false);
			}
			//enable/disable reflections only in Horizontal mode
			if(viewMode == ViewMode.grid || viewMode == ViewMode.vertical)
			{
				reflectArray[i].SetActive(false);
			}else if(viewMode == ViewMode.horizontal){
				reflectArray[i].SetActive(true);
			}
		}
	}

	void ElementsRotation()
	{

		for(int i = 0; i<= gallery.Length -1 ; i++)
		{

			gallery[i].transform.rotation = Quaternion.Euler(90, 180 + rotationOffset, 0);
		}
	}
	


	void GetTextures()
	{
		//Creates an array of planes in the same size as the textures array
		gallery = new GameObject[textures.Length];
		//Creates an array of planes for the reflexes
		reflectArray = new GameObject[textures.Length];
		//Creates an array of names in the same size as the gallery array
		names = new GameObject[gallery.Length];
		for(int i = 0; i<= gallery.Length -1 ; i++)
		{
			gallery[i] = GameObject.CreatePrimitive(PrimitiveType.Plane);
			gallery[i].GetComponent<Renderer>().material = Resources.Load("TEXTURE FLOW/Materials/Unlit_Transparent") as Material;
			
			//now it will hold all textures from the target directory and adjust their rotation so they face the camera
			gallery[i].GetComponent<Renderer>().material.mainTexture = textures[i] as Texture;
			gallery[i].transform.rotation = Quaternion.Euler(90, 180, 0);

			//creates reflections
			GameObject reflection = Instantiate(gallery[i], new Vector3(0, 0, 10), Quaternion.identity ) as GameObject;
			reflection.transform.parent = gallery[i].transform;
			reflection.GetComponent<Collider>().enabled = false;
			reflection.GetComponent<Renderer>().material = Resources.Load("TEXTURE FLOW/Materials/AlphaMask") as Material;
			reflection.GetComponent<Renderer>().material.mainTexture = textures[i] as Texture;
			reflection.transform.position = new Vector3(0, -10, 0);
			reflection.transform.rotation = Quaternion.Euler(90, 180, 0);
			reflection.transform.localScale = new Vector3(1, 1, -1);
			//keeps reflections in an array
			reflectArray[i] = reflection;

			//put names on the images
			//rename planes
			gallery[i].name = "img" + i.ToString();
			//parent names to each respective image
			names[i] = new GameObject();
			names[i].transform.parent = gallery[i].transform;
	
			//add font material
			names[i].AddComponent<TextMesh>();
			names[i].GetComponent<TextMesh>().font = targetFont;
			names[i].GetComponent<Renderer>().material = Resources.Load("TEXTURE FLOW/Materials/Font_Material") as Material;
			names[i].GetComponent<Renderer>().material.mainTexture = targetFont.material.mainTexture;

		}
	}

	void CreateTitle()
	{
		TitleName = new GameObject();
		TitleName.transform.parent = this.gameObject.transform;
		TitleName.name = "Title";
		//add font material
		TitleName.AddComponent<GUIText>();
		TitleName.GetComponent<GUIText>().font = targetFont;
		TitleName.GetComponent<GUIText>().material = Resources.Load("TEXTURE FLOW/Materials/Font_Material") as Material;
		TitleName.GetComponent<GUIText>().material.mainTexture = targetFont.material.mainTexture;
		TitleName.GetComponent<GUIText>().anchor = TextAnchor.LowerRight;
		TitleName.GetComponent<GUIText>().alignment = TextAlignment.Right;
		TitleName.GetComponent<GUIText>().fontSize = 14;
		TitleName.transform.localPosition = new Vector3(0.99f, 0.01f, 0);
		TitleName.GetComponent<GUIText>().text = title + System.Environment.NewLine + extraTitle;
	}

	void Rename()
	{
		//Tile
		if(showTitle)
		{
			TitleName.SetActive(true);
			TitleName.GetComponent<GUIText>().text = title + System.Environment.NewLine + extraTitle;
		}
		else
		{
			TitleName.SetActive(false);
		}

		//BEGIN check names
		//------------------------------------------------------------------------------
		for(int i = 0; i<= gallery.Length -1 ; i++)
		{
			//wether it will show names or not (DonDisplay)
			if(displayNames == DisplayNames.dontDisplay)
			{
				names[i].SetActive(false);
			}else{
				names[i].SetActive(true);
			}
			//display names according to options  (Names/Numbes)
			if(displayNames == DisplayNames.names)
			{
				names[i].GetComponent<TextMesh>().text = textures[i].name;
			}
			if(displayNames == DisplayNames.numbers)
			{
				names[i].GetComponent<TextMesh>().text = (i + 1 ).ToString();
			}

			//scale down
			names[i].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

			//Display names according to view mode VERTICAL/HORIZONTAL/GRID
			if(viewMode == ViewMode.grid || viewMode == ViewMode.horizontal)
			{
				names[i].GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
				names[i].GetComponent<TextMesh>().alignment = TextAlignment.Center;
				names[i].transform.localPosition = new Vector3(0, 0, -8);
				names[i].transform.rotation = Quaternion.Euler(0, 0, 0);

			}else if(viewMode == ViewMode.vertical){
				names[i].GetComponent<TextMesh>().anchor = TextAnchor.MiddleLeft;
				names[i].GetComponent<TextMesh>().alignment = TextAlignment.Left;
				names[i].transform.localPosition = new Vector3(-8, 0, 0);
				names[i].transform.rotation = Quaternion.Euler(0, 0, 0);

			}



		}
		//END check names
		//------------------------------------------------------------------------------
	}

	void SpaceOut()
	{
		initialColumns = columns;
		for(int i = 0; i<= gallery.Length -1 ; i++)
		{
			//BEGIN adjust spacing
			//------------------------------------------------------------------------------
			//the first element will always start in the origin point spefied in the inspector(Vector3)
			gallery[0].transform.position = gridOrigin;
			
			//adjust spacing between planes
			//ignore the first element of the array (ORIGIN element) and start adjusting from #2
			if(i > 0)
			{
				//it will now create spacing between the images
				//HORIZONTAL
				if(viewMode == ViewMode.horizontal){
					gallery[i].transform.position = new Vector3(gallery[i-1].transform.position.x + spacing, gridOrigin.y, gridOrigin.z);
				}
				//VERTICAL
				if(viewMode == ViewMode.vertical){
					gallery[i].transform.position = new Vector3(gridOrigin.x, gallery[i-1].transform.position.y - spacing, gridOrigin.z);
				}
				//GRID
				if(viewMode == ViewMode.grid){
					if(columns > 1)
					{
						gallery[i].transform.position = new Vector3(gallery[i-1].transform.position.x + spacing, gallery[i-1].transform.position.y, gridOrigin.z);
						columns -= 1;
					}else{
						columns = initialColumns;
						gallery[i].transform.position = new Vector3(gridOrigin.x, gallery[i-1].transform.position.y - spacing, gridOrigin.z);
					}
				}
			}
			//create a limit so the camera doesnt go off the screen
			GalleryBounds();
			//Find camera and adjusts it in accordance to the images
			AdjustCamera();


			//END adjust spacing
			//------------------------------------------------------------------------------
		}
		columns = initialColumns;
	}


	void CameraMove()
	{
		//KEYBOARD INPUT
		float h = 10f * Input.GetAxis("Horizontal");
		float v = 10f * Input.GetAxis("Vertical");

		//moves camera at the speed of 5f
		Vector3 newPosition = camera.transform.position; //assign new position so it can be corrected via LERP

		//HORIZONTAL
		if(viewMode == ViewMode.horizontal){
			if(camera.transform.position.x > beginBound.x && camera.transform.position.x < endBound.x)//if inside bounds limits then move the camera
			{
				newPosition = new Vector3(camera.transform.position.x + h, camera.transform.position.y, camera.transform.position.z);
			}else if(camera.transform.position.x < endBound.x){ //applies correction
				newPosition = new Vector3 (gallery[0].transform.position.x, camera.transform.position.y, camera.transform.position.z);
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
			}else if(camera.transform.position.x > endBound.x){//applies correction
				newPosition = new Vector3 (gallery[gallery.Length-1].transform.position.x, camera.transform.position.y, camera.transform.position.z);
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
			}	
		}
		if(viewMode == ViewMode.vertical || viewMode == ViewMode.grid){
			//camera.transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y + v, camera.transform.position.z);
			if(camera.transform.position.y < beginBound.y && camera.transform.position.y > endBound.y)//if inside bounds limits then move the camera
			{
				newPosition = new Vector3(camera.transform.position.x, camera.transform.position.y + v, camera.transform.position.z);
			}else if(camera.transform.position.y > endBound.y){ //applies correction
				newPosition = new Vector3 (camera.transform.position.x, gallery[0].transform.position.y, camera.transform.position.z);
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
			}else if(camera.transform.position.y < endBound.y){//applies correction
				newPosition = new Vector3 (camera.transform.position.x, gallery[gallery.Length-1].transform.position.y, camera.transform.position.z);
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
			}	
		}
		//LERP CAMERA
		camera.transform.position = Vector3.Lerp(camera.transform.position, newPosition, 10 * Time.deltaTime);
	}

	void GalleryBounds()
	{

		Vector3 offSet;

		if(viewMode == ViewMode.horizontal)
		{	
			offSet = new Vector3(10,0,0);
		}else{
			offSet = new Vector3(0,10,0);
		}
		if(viewMode == ViewMode.horizontal){
			beginBound = gallery[0].transform.position - offSet;
			endBound = gallery[gallery.Length-1].transform.position + offSet;
		}else{
			beginBound = gallery[0].transform.position + offSet;
			endBound = gallery[gallery.Length-1].transform.position - offSet;
		}
	}


	
	void AdjustCamera()
	{
		//Finds Main Camera
		camera = Camera.main;
		camera.transform.position = new Vector3(0,0,0);
		//adjusts camera's rotation so it faces the textures
		//camera.transform.rotation = Quaternion.Euler(0, 0, 0);
		//adjusts camera
		GameObject centerElement = gallery[Mathf.Abs(gallery.Length/2)];

		//adjusts camera start
		switch(cameraStart)
		{
			case CameraStart.Middle:
			if(viewMode == ViewMode.grid){
					//if in grid mode, camera's initial position will be relative to the center of the columns.
					camera.transform.position = new Vector3(Mathf.Abs(gallery[0].transform.position.x + gallery[initialColumns-1].transform.position.x)/2, centerElement.transform.position.y, camera.transform.position.z - cameraDistance);
				}else{
					//if not in grid mode, camera's intial position will be according to the center of the textures (in line)
					camera.transform.position = new Vector3(centerElement.transform.position.x, centerElement.transform.position.y, camera.transform.position.z - cameraDistance);
				}
			break;
			case CameraStart.Beginning:
			if(viewMode == ViewMode.grid){
				camera.transform.position = new Vector3(Mathf.Abs(gallery[0].transform.position.x + gallery[initialColumns-1].transform.position.x)/2, gallery[initialColumns+1].transform.position.y, camera.transform.position.z - cameraDistance);
				}else{
					camera.transform.position = new Vector3(gallery[2].transform.position.x, gallery[2].transform.position.y, camera.transform.position.z - cameraDistance);
				}
			break;
			case CameraStart.End:
			if(viewMode == ViewMode.grid){
					camera.transform.position = new Vector3(Mathf.Abs(gallery[0].transform.position.x + gallery[initialColumns-1].transform.position.x)/2, gallery[gallery.Length-2].transform.position.y, camera.transform.position.z - cameraDistance);
				}else{
					camera.transform.position = new Vector3(gallery[gallery.Length-3].transform.position.x, gallery[gallery.Length-3].transform.position.y, camera.transform.position.z - cameraDistance);
				}
			break;
		}
	}


	void CameraZoom()
	{
			//change origin of raycast according to viewMode
			Vector3 rayCastPosition;
			if(viewMode == ViewMode.vertical)
			{
				Vector3 targetX = camera.WorldToViewportPoint(gridOrigin);
				rayCastPosition = new Vector3(targetX.x ,0.5f, 0);
			}else{
				rayCastPosition = new Vector3(0.5F, 0.5F, 0);
			}
			
			//magnification effect
			Ray ray = camera.ViewportPointToRay(rayCastPosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)){

				if(hit.transform.gameObject != focalElement && focalElement != null) //verifies if hit object changed
				{
					focalElement = null; //resets so it can reassign obj to this variable and run the loop again.
				}

				if(focalElement == null)//executes only once 
				{	focalElement = hit.transform.gameObject; //now it wont do it again
					focalElementView = hit.transform.gameObject;
					for(int i = 0; i <= gallery.Length -1 ; i++)
					{
						if(gallery[i].name == hit.transform.name)
						{
							//focal element is bigger
							//gallery[i].transform.localScale = new Vector3 (2f, 2f, 2f);
							//adjacent elements to the focal element are just slightly big
							if(i != gallery.Length -1 && i != 0){ //if not the first and last elements of array (so it doesnt exceed bounds)
								//gallery[i-1].transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
								//gallery[i+1].transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f); 
								adjacentLeft = gallery[i-1].gameObject;
								adjacentRight = gallery[i+1].gameObject;
								//finds the previously adjacent elements so they can be shrinked down again
								if(i != gallery.Length -2 && i != 1) //if first and last elements of array (so it doesnt exceed bounds)
								{
									previousAdjLeft = gallery[i-2].gameObject;
									previousAdjRight = gallery[i+2].gameObject;
								}
								else if(i == gallery.Length -2){ 
									previousAdjLeft = gallery[i-2].gameObject;
									previousAdjRight = gallery[i+1].gameObject;
								}
								else if(i == 1){ 
									previousAdjLeft = gallery[i-1].gameObject;
									previousAdjRight = gallery[i+2].gameObject;
								}
							}
							else if(i == gallery.Length -1){ 
								adjacentLeft = gallery[i-1].gameObject;
								previousAdjLeft = gallery[i-2].gameObject;
								adjacentRight = gallery[0].gameObject;//just so it doesnt exceed the bounds of array, make it go back to 0
								previousAdjRight = gallery[1].gameObject;//next after the above
								
								
							}
							else if(i == 0){ 
								adjacentLeft = gallery[gallery.Length-1].gameObject; //just so it doesnt exceed the bounds of array, make it go to last element
								previousAdjLeft = gallery[gallery.Length-2].gameObject;	//next after the above
								adjacentRight = gallery[i+1].gameObject;
								previousAdjRight = gallery[i+2].gameObject;
							}
							
						}
					}
					//now the rest of the elements are scalled back to normal size (needs to be in a separate loop because the first runs only once and dont go back to the previously processed elements)
					for(int k = 0; k <= gallery.Length -1 ; k++)
					{
						if(gallery[k].name != hit.transform.name)
						{
							if(gallery[k] != adjacentLeft && gallery[k] != adjacentRight && gallery[k] != focalElement && gallery[k] != previousAdjLeft && gallery[k] != previousAdjRight)
							{
								gallery[k].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
							}
						}
					}
				}
	
			}
			//BEGIN Apply ZOOM IN or ZOOM OUT
			//------------------------------------------------------
			//ZOOM IN
			//easing focal element
			if(focalElementView != null)
			{
				focalElementView.transform.localScale = Vector3.Lerp(focalElementView.transform.localScale, new Vector3 (2f, 2f, 2f), 4 * Time.deltaTime );
			}
			//ZOOM OUT
			previousAdjLeft.transform.localScale = Vector3.Lerp(previousAdjLeft.transform.localScale, new Vector3 (1.0f, 1.0f, 1.0f), 10 * Time.deltaTime ); //when adj needs to go back to 1
			previousAdjRight.transform.localScale = Vector3.Lerp(previousAdjRight.transform.localScale, new Vector3 (1.0f, 1.0f, 1.0f), 10 * Time.deltaTime ); //when adj needs to go back to 1
			
			
			//verifies if object is off of center of screen and magnifies the next element in the queue
			Vector3 centerElement;
			if(viewMode == ViewMode.horizontal){
				centerElement = camera.WorldToScreenPoint(focalElementView.transform.position)/Screen.width;
				//XXXXXXX
				if((centerElement.x > 0.45f && centerElement.x < 0.55f ))
				{
					ZoomInAdjacents();
					
				}else if(centerElement.x > 0.55f)//if it's going to RIGHT
				{
					ZoomInLeftAdj();
					
				}else if(centerElement.x < 0.45f )//if it's going to LEFT
				{
					ZoomInRightAdj();
					
				}
			}else{
				centerElement = camera.WorldToScreenPoint(focalElementView.transform.position)/Screen.height;
				//YYYYYYY
				if((centerElement.y > 0.45f && centerElement.y < 0.55f ))
				{
					ZoomInAdjacents();
					
				}else if(centerElement.y > 0.55f)//if it's going to RIGHT
				{
					ZoomInRightAdj();
					
				}else if(centerElement.y < 0.45f )//if it's going to LEFT
				{
					
					ZoomInLeftAdj();
					
				}
			}
			
			//END Apply ZOOM IN or ZOOM OUT
			//------------------------------------------------------
	}


	void ZoomInAdjacents()
	{
		//zooming/easing adjacent element on left
		adjacentLeft.transform.localScale = Vector3.Lerp(adjacentLeft.transform.localScale, new Vector3 (1.4f, 1.4f, 1.4f), 4 * Time.deltaTime );
		//zooming/easing adjacent element on right
		adjacentRight.transform.localScale = Vector3.Lerp(adjacentRight.transform.localScale, new Vector3 (1.4f, 1.4f, 1.4f), 4 * Time.deltaTime );
	}
	void ZoomInLeftAdj()
	{
		//zooming/easing adjacent element on left
		adjacentLeft.transform.localScale = Vector3.Lerp(adjacentLeft.transform.localScale, new Vector3 (1.7f, 1.7f, 1.7f), 4 * Time.deltaTime );
		//zooming/easing adjacent element on right
		adjacentRight.transform.localScale = Vector3.Lerp(adjacentRight.transform.localScale, new Vector3 (1.4f, 1.4f, 1.4f), 4 * Time.deltaTime );
	}
	void ZoomInRightAdj()
	{
		//zooming/easing adjacent element on left
		adjacentLeft.transform.localScale = Vector3.Lerp(adjacentLeft.transform.localScale, new Vector3 (1.4f, 1.4f, 1.4f), 4 * Time.deltaTime );
		//zooming/easing adjacent element on right
		adjacentRight.transform.localScale = Vector3.Lerp(adjacentRight.transform.localScale, new Vector3 (1.7f, 1.7f, 1.7f), 4 * Time.deltaTime );
	}


	
	void MouseZoom()
	{
		//magnification effect
		Ray ray = camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)){

			if(hit.transform.gameObject != focalElement && focalElement != null) //verifies if hit object changed
			{
				previouslyViewed = focalElement;
				focalElement = null; //resets so it can reassign obj to this variable and run the loop again.
			}
			
			if(focalElement == null)//executes only once 
			{	focalElement = hit.transform.gameObject; //now it wont do it again

				//now the rest of the elements are scalled back to normal size (needs to be in a separate loop because the first runs only once and dont go back to the previously processed elements)
				for(int k = 0; k <= gallery.Length -1 ; k++)
				{
					if(gallery[k].name != hit.transform.name)
					{
						if(gallery[k] != previouslyViewed)
						{
							gallery[k].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
						}
					}
				}
			}
			//ZOOM IN
			//easing focal element
			hit.transform.localScale = Vector3.Lerp(hit.transform.localScale, new Vector3 (2f, 2f, 2f), 6 * Time.deltaTime );
			//ZOOM OUT
			//easing adjacent element on left
			if(previouslyViewed != focalElement && previouslyViewed != null)
			{
				previouslyViewed.transform.localScale = Vector3.Lerp(previouslyViewed.transform.localScale, new Vector3 (1f, 1f, 1f), 10 * Time.deltaTime );
			}
		}else{// if mouse isnt hovering any element at all then shrinks everything back to normal size
			previouslyViewed = focalElement;
			if(previouslyViewed != null){
				previouslyViewed.transform.localScale = Vector3.Lerp(previouslyViewed.transform.localScale, new Vector3 (1f, 1f, 1f), 10 * Time.deltaTime );
			}
		}
	}

	void OnGUI (){

		//ALIGNMENT BUTTONS
		GUI.enabled = true;
		if(alignmentOptions)
		{
			if(GUI.Button(new Rect(Screen.width/2-144,Screen.height-32,96,16), "Grid")) {
				viewMode = ViewMode.grid;
				ApplyChanges();
			}
			if(GUI.Button(new Rect(Screen.width/2-48,Screen.height-32,96,16), "Horizontal")) {
				viewMode = ViewMode.horizontal;
				ApplyChanges();
			}
			if(GUI.Button(new Rect(Screen.width/2+48,Screen.height-32,96,16), "Vertical")) {
				viewMode = ViewMode.vertical;
				ApplyChanges();
			}
		}


		// SCROLL BAR
		float galleryExtensionX = gallery[gallery.Length-1].transform.position.x - gallery[0].transform.position.x;	
		float galleryExtensionY = gallery[gallery.Length-1].transform.position.y - gallery[0].transform.position.y;

		if(viewMode == ViewMode.horizontal){
			float vBarValueX = GUI.HorizontalScrollbar(new Rect(Screen.width/2-144, Screen.height-64, 288 , 30), camera.transform.position.x - gridOrigin.x, 1.0F, 10.0F, galleryExtensionX);

			if (GUI.changed) {
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
				camera.transform.position = new Vector3(vBarValueX + gridOrigin.x,camera.transform.position.y,camera.transform.position.z);
				cameraDrag.enabled = false; // deactivates drag so it prevents the screen from moving after changing the slider position
			} 
		}else{
			float vBarValueY = GUI.VerticalScrollbar(new Rect(Screen.width - 32, Screen.height/2-144, 30 , 288), camera.transform.position.y - gridOrigin.y, 1.0F, 10.0F, galleryExtensionY);
			
			if (GUI.changed) {
				cameraDrag.deltaPos = new Vector3(0,0,0); //nulifies camera movement in the CameraDrag Script
				camera.transform.position = new Vector3(camera.transform.position.x,vBarValueY + gridOrigin.y,camera.transform.position.z);
				cameraDrag.enabled = false; // deactivates drag so it prevents the screen from moving after changing the slider position
			} 
		}

		//vSbarValue = GUI.VerticalScrollbar(new Rect(32, 32, 100, 30), vSbarValue, 1.0F, 10.0F, 0.0F);


	
	}
	//reactivates camera drag after next click of the mouse
	void ReactivatesCameraDrag()
	{
		if(Input.GetMouseButtonDown(0))
		{
			cameraDrag.enabled = true;
		}
	}


	
}
