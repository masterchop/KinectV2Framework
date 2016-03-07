using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomEditor(typeof(Gallery)), CanEditMultipleObjects]
public class GalleryEditor : Editor {

	public SerializedProperty 
		viewMode_Prop,
		columns_Prop;
	
	void OnEnable () {
		// Setup the SerializedProperties
		viewMode_Prop = serializedObject.FindProperty ("viewMode");
		columns_Prop = serializedObject.FindProperty ("columns");        
	}


	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

	
		serializedObject.Update ();
		
		EditorGUILayout.PropertyField( viewMode_Prop );
		
		Gallery.ViewMode vm = (Gallery.ViewMode)viewMode_Prop.enumValueIndex;
		
		switch( vm ) {
		case Gallery.ViewMode.grid: 
			//show columns
			EditorGUILayout.PropertyField( columns_Prop, new GUIContent("Columns") );            
			break;
			
		case Gallery.ViewMode.horizontal:            
			//dont show columns
			break;
			
		case Gallery.ViewMode.vertical:               
			//dont show columns
			break;
			
		}

		Gallery gallery = (Gallery)target;
		if(GUILayout.Button ("Refresh"))
		{
			try{
				gallery.ApplyChanges();
			}
			catch(Exception e){
				Debug.Log("Changes are only previewed in game mode.");
			}
		}
		//		if(GUILayout.Button ("Load"))
		//		{
		//			gallery.CreateNew();
		//		}
		
		
		serializedObject.ApplyModifiedProperties ();
	}

//	[MenuItem("GameObject/Create Material")]
//	public static void CreateDemoObjects()
//	{
//		// Create a simple material asset
//		var material = new Material (Shader.Find("Specular"));
//		AssetDatabase.CreateAsset(material, "Assets/MyMaterial.mat");
//		// Print the path of the created asset
//		Debug.Log(AssetDatabase.GetAssetPath(material));
//	}

		[MenuItem("GameObject/Texture Flow/Create New Gallery")]
		public static void CreateGallery()
		{
			// Create Gallery
			GameObject Gallery = Instantiate(Resources.Load ("TEXTURE FLOW/Prefabs/Gallery"), new Vector3(0,0,0), Quaternion.identity) as GameObject;
			Gallery.name = "Gallery";

		}


}
