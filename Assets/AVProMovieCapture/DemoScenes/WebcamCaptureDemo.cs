using UnityEngine;
using System.Collections;

public class WebcamCaptureDemo : MonoBehaviour 
{
#if UNITY_4_0 || UNITY_3_5
	public AVProMovieCaptureFromTexture _movieCapture;
	public GUISkin _skin;
	private WebCamTexture _texture;
	private int _webcamIndex = -1;
	private GUIContent[] _webcamNames;
	
	void Start() 
	{	
		_webcamNames = new GUIContent[WebCamTexture.devices.Length];
		for (int i = 0 ; i < _webcamNames.Length; i++)
		{
			_webcamNames[i] = new GUIContent(WebCamTexture.devices[i].name);
		}
		
		SelectWebcam(0);
	}
	
	private void SelectWebcam(int webcamIndex)
	{
		if (webcamIndex < WebCamTexture.devices.Length)
		{
			if (_texture)
			{
				_texture.Stop();
				Destroy(_texture);
				_texture = null;
			}
			_webcamIndex = -1;
			_texture = new WebCamTexture(WebCamTexture.devices[webcamIndex].name, 640, 480, 30);
			_texture.Play();
			if (_texture.isPlaying)
			{
				_webcamIndex = webcamIndex;
				if (_movieCapture)
				{
					// WebCamTexture actually uses a power of 2 texture so we need to only grab a region of it
					float p2Width = Mathf.NextPowerOfTwo(_texture.width);
					float p2Height = Mathf.NextPowerOfTwo(_texture.height);
					
					_movieCapture.SetSourceTextureRegion(_texture, new Rect(0, 0, _texture.width / p2Width, _texture.height / p2Height));
				}
			}
		}
	}
	
	void OnDestroy()
	{
		if (_texture)
		{
			_texture.Stop();
			Destroy(_texture);
			_texture = null;
		}
	}
	
	void OnGUI()
	{
		GUI.skin = _skin;
		GUILayout.BeginArea(new Rect(Screen.width - 256, 0, 256, Screen.height));
		GUILayout.BeginVertical();
		GUILayout.Label("Webcams:");
		int webcamIndex = GUILayout.SelectionGrid(_webcamIndex, _webcamNames, 1);
		if (webcamIndex != _webcamIndex)
		{
			SelectWebcam(webcamIndex);
		}
		if (_texture)
		{
			Rect camRect = GUILayoutUtility.GetRect(256, 256.0f / (_texture.width / (float)_texture.height));
			GUI.DrawTexture(camRect, _texture);
		}
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
#endif
}
