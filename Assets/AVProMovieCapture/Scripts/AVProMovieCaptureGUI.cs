using UnityEngine;
using System.Text;
using System.Collections;

//-----------------------------------------------------------------------------
// Copyright 2012-2013 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[AddComponentMenu("AVPro Movie Capture/GUI")]
public class AVProMovieCaptureGUI : MonoBehaviour 
{
	public AVProMovieCaptureBase _movieCapture;
	public bool _showUI = true;
	public bool _whenRecordingAutoHideUI = true;
	public GUISkin _guiSkin;
	
	// GUI
	private int _shownSection = -1;
	private string[] _codecNames;
	private string[] _audioCodecNames;
	private string[] _audioDeviceNames;
	private string[] _downScales;
	private string[] _frameRates;
	private int _downScaleIndex;
	private int _frameRateIndex;
	private Vector2 _videoPos = Vector2.zero;
	private Vector2 _audioPos = Vector2.zero;
	private Vector2 _audioCodecPos = Vector2.zero;
	
	void Start()
	{
		CreateGUI();
	}
		
	private void CreateGUI()
	{
		_downScales = new string[6];
		_downScales[0] = "Original";
		_downScales[1] = "Half";
		_downScales[2] = "Quarter";
		_downScales[3] = "Eighth";
		_downScales[4] = "Sixteenth";
		_downScales[5] = "Specific";
		switch (_movieCapture._downScale)
		{
		default:
		case AVProMovieCaptureBase.DownScale.Original:
			_downScaleIndex = 0;	
			break;
		case AVProMovieCaptureBase.DownScale.Half:
			_downScaleIndex = 1;	
			break;
		case AVProMovieCaptureBase.DownScale.Quarter:
			_downScaleIndex = 2;	
			break;
		case AVProMovieCaptureBase.DownScale.Eighth:
			_downScaleIndex = 3;	
			break;
		case AVProMovieCaptureBase.DownScale.Sixteenth:
			_downScaleIndex = 4;	
			break;
		case AVProMovieCaptureBase.DownScale.Specific:
			_downScaleIndex = 5;
			break;
		}
		
		_frameRates = new string[5];
		_frameRates[0] = "15";
		_frameRates[1] = "24";
		_frameRates[2] = "25";
		_frameRates[3] = "30";
		_frameRates[4] = "60";
		switch (_movieCapture._frameRate)
		{
		default:
		case AVProMovieCaptureBase.FrameRate.Fifteen:
			_frameRateIndex = 0;
			break;
		case AVProMovieCaptureBase.FrameRate.TwentyFour:
			_frameRateIndex = 1;
			break;
		case AVProMovieCaptureBase.FrameRate.TwentyFive:
			_frameRateIndex = 2;
			break;
		case AVProMovieCaptureBase.FrameRate.Thirty:
			_frameRateIndex = 3;
			break;
		case AVProMovieCaptureBase.FrameRate.Sixty:
			_frameRateIndex = 4;
			break;
		}

		int numVideoCodecs = AVProMovieCapturePlugin.GetNumAVIVideoCodecs();
		if (numVideoCodecs > 0)
		{
			_codecNames = new string[numVideoCodecs+1];
			_codecNames[0] = "Uncompressed";
			for (int i = 0; i < numVideoCodecs; i++)
			{
				_codecNames[i+1] = AVProMovieCapturePlugin.GetAVIVideoCodecName(i);
			}
		}

		int numAudioDevices = AVProMovieCapturePlugin.GetNumAVIAudioInputDevices();
		if (numAudioDevices > 0)
		{
			_audioDeviceNames = new string[numAudioDevices+1];
			_audioDeviceNames[0] = "Unity";
			for (int i = 0; i < numAudioDevices; i++)
			{
				_audioDeviceNames[i + 1] = AVProMovieCapturePlugin.GetAVIAudioInputDeviceName(i);
			}
		}

		int numAudioCodecs = AVProMovieCapturePlugin.GetNumAVIAudioCodecs();
		if (numAudioCodecs > 0)
		{
			Debug.Log("num codecs " + numAudioCodecs);
			_audioCodecNames = new string[numAudioCodecs+1];
			_audioCodecNames[0] = "Uncompressed";
			for (int i = 0; i < numAudioCodecs; i++)
			{
				_audioCodecNames[i + 1] = AVProMovieCapturePlugin.GetAVIAudioCodecName(i);
			}
		}		

		_movieCapture.SelectCodec(false);
		_movieCapture.SelectAudioCodec(false);
		_movieCapture.SelectAudioDevice(false);
	}

	void OnGUI()
	{
		GUI.skin = _guiSkin;
		GUI.depth = -10;
		
		if (_showUI)
			GUILayout.Window(4, new Rect(0, 0, 450, 256), MyWindow, "AVPro Movie Capture UI");
	}

	void MyWindow(int id)
	{
		GUILayout.BeginVertical();
		
		if (_movieCapture != null)
		{		
			GUILayout.Label("Resolution:");
			GUILayout.BeginHorizontal();
			_downScaleIndex = GUILayout.SelectionGrid(_downScaleIndex, _downScales, _downScales.Length);
			switch (_downScaleIndex)
			{
				case 0:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Original;
					break;
				case 1:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Half;
					break;
				case 2:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Quarter;
					break;
				case 3:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Eighth;
					break;
				case 4:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Sixteenth;
					break;
				case 5:
					_movieCapture._downScale = AVProMovieCaptureBase.DownScale.Specific;
					break;
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal(GUILayout.Width(256));
			if (_movieCapture._downScale == AVProMovieCaptureBase.DownScale.Specific)
			{
				string maxWidthString = GUILayout.TextField(Mathf.FloorToInt(_movieCapture._maxVideoSize.x).ToString(), 4);
				int maxWidth = 0;
				if (int.TryParse(maxWidthString, out maxWidth))
				{
					_movieCapture._maxVideoSize.x = Mathf.Clamp(maxWidth, 0, 4096);
				}
				
				GUILayout.Label("x", GUILayout.Width(20));
					
				string maxHeightString = GUILayout.TextField(Mathf.FloorToInt(_movieCapture._maxVideoSize.y).ToString(), 4);
				int maxHeight = 0;
				if (int.TryParse(maxHeightString, out maxHeight))
				{
					_movieCapture._maxVideoSize.y = Mathf.Clamp(maxHeight, 0, 4096);
				}
			}
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Frame Rate:");
			_frameRateIndex = GUILayout.SelectionGrid(_frameRateIndex, _frameRates, _frameRates.Length);
			switch (_frameRateIndex)
			{
				case 0:
					_movieCapture._frameRate = AVProMovieCaptureBase.FrameRate.Fifteen;
					break;
				case 1:
					_movieCapture._frameRate = AVProMovieCaptureBase.FrameRate.TwentyFour;
					break;
				case 2:
					_movieCapture._frameRate = AVProMovieCaptureBase.FrameRate.TwentyFive;
					break;
				case 3:
					_movieCapture._frameRate = AVProMovieCaptureBase.FrameRate.Thirty;
					break;
				case 4:
					_movieCapture._frameRate = AVProMovieCaptureBase.FrameRate.Sixty;
					break;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(16f);
			
			_movieCapture._isRealTime = GUILayout.Toggle(_movieCapture._isRealTime, "RealTime");
			
			GUILayout.Space(16f);
			
			
			
			
			// Video Codec
			GUILayout.BeginHorizontal();
			if (_shownSection != 0)
			{
				if (GUILayout.Button("+", GUILayout.Width(24)))
				{
					_shownSection = 0;
				}
			}
			else
			{
				if (GUILayout.Button("-", GUILayout.Width(24)))
				{
						_shownSection = -1;
				}
			}
			GUILayout.Label("Using Video Codec: " + _movieCapture._codecName);
			if (_movieCapture._codecIndex >= 0)
			{	
				GUILayout.Space(16f);
				if (GUILayout.Button("Configure Codec"))
				{
					AVProMovieCapturePlugin.ConfigureVideoCodec(_movieCapture._codecIndex);
				}
			}			
			GUILayout.EndHorizontal();
				
			if (_codecNames != null && _shownSection == 0)
			{
				GUILayout.Label("Select Video Codec:");
				_videoPos = GUILayout.BeginScrollView(_videoPos, GUILayout.Height(100));
				int newCodecIndex = GUILayout.SelectionGrid(-1, _codecNames, 1) - 1;
				GUILayout.EndScrollView();
				
				if (newCodecIndex >= -1)
				{
					_movieCapture._codecIndex = newCodecIndex;
					if (_movieCapture._codecIndex >= 0)
						_movieCapture._codecName = _codecNames[_movieCapture._codecIndex + 1];
					else
						_movieCapture._codecName = "Uncompressed";
					
					_shownSection = -1;
				}
					
				GUILayout.Space(16f);
			}
			
			
			_movieCapture._noAudio = !GUILayout.Toggle(!_movieCapture._noAudio, "Record Audio");
			GUI.enabled = !_movieCapture._noAudio;
			
			// Audio Device
			GUILayout.BeginHorizontal();
			if (_shownSection != 1)
			{
				if (GUILayout.Button("+", GUILayout.Width(24)))
				{
					_shownSection = 1;
				}
			}
			else
			{
				if (GUILayout.Button("-", GUILayout.Width(24)))
				{
					_shownSection = -1;
				}
			}			
			GUILayout.Label("Using Audio Source: " + _movieCapture._audioDeviceName);
			GUILayout.EndHorizontal();
			if (_audioDeviceNames != null && _shownSection == 1)
			{
				GUILayout.Label("Select Audio Source:");
				_audioPos = GUILayout.BeginScrollView(_audioPos, GUILayout.Height(100));
				int newAudioIndex = GUILayout.SelectionGrid(-1, _audioDeviceNames, 1) - 1;
				GUILayout.EndScrollView();
				
				if (newAudioIndex >= -1)
				{
					_movieCapture._audioDeviceIndex = newAudioIndex;
					if (_movieCapture._audioDeviceIndex >= 0)
						_movieCapture._audioDeviceName = _audioDeviceNames[_movieCapture._audioDeviceIndex + 1];
					else
						_movieCapture._audioDeviceName = "Unity";
					
					_shownSection = -1;
				}

				GUILayout.Space(16f);
			}
			
			
			
			// Audio Codec
			GUILayout.BeginHorizontal();
			if (_shownSection != 2)
			{
				if (GUILayout.Button("+", GUILayout.Width(24)))
				{
					_shownSection = 2;
				}
			}
			else
			{
				if (GUILayout.Button("-", GUILayout.Width(24)))
				{
						_shownSection = -1;
				}
			}
			GUILayout.Label("Using Audio Codec: " + _movieCapture._audioCodecName);
			if (_movieCapture._audioCodecIndex >= 0)
			{	
				GUILayout.Space(16f);
				if (GUILayout.Button("Configure Codec"))
				{
					AVProMovieCapturePlugin.ConfigureAudioCodec(_movieCapture._audioCodecIndex);
				}
			}			
			GUILayout.EndHorizontal();
				
			if (_audioCodecNames != null && _shownSection == 2)
			{
				GUILayout.Label("Select Audio Codec:");
				_audioCodecPos = GUILayout.BeginScrollView(_audioCodecPos, GUILayout.Height(100));
				int newCodecIndex = GUILayout.SelectionGrid(-1, _audioCodecNames, 1) - 1;
				GUILayout.EndScrollView();
				
				if (newCodecIndex >= -1)
				{
					_movieCapture._audioCodecIndex = newCodecIndex;
					if (_movieCapture._audioCodecIndex >= 0)
						_movieCapture._audioCodecName = _audioCodecNames[_movieCapture._audioCodecIndex + 1];
					else
						_movieCapture._audioCodecName = "Uncompressed";
					
					_shownSection = -1;
				}
					
				GUILayout.Space(16f);
			}
			
			GUI.enabled = true;
			
			GUILayout.Space(16f);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Filename Prefix: ");
			_movieCapture._autoFilenamePrefix = GUILayout.TextField(_movieCapture._autoFilenamePrefix, 64);
			GUILayout.EndHorizontal();
			GUILayout.Space(16f);
			GUILayout.Space(16f);
			
			GUILayout.Label("(Press Esc or CTRL-F5 to stop capture)");
			
			GUILayout.BeginHorizontal();
			if (!_movieCapture.IsCapturing())
			{
				if (GUILayout.Button("Start Capture"))
				{
					if (_whenRecordingAutoHideUI)
						_showUI = false;
					_movieCapture.StartCapture();
				}
			}
			else
			{
				if (!_movieCapture.IsPaused())
				{
					if (GUILayout.Button("Pause Capture"))
					{
						_movieCapture.PauseCapture();
					}
				}
				else
				{
					if (GUILayout.Button("Resume Capture"))
					{
						_movieCapture.ResumeCapture();
					}					
				}
				
				if (GUILayout.Button("Stop Capture"))
				{
					_movieCapture.StopCapture();
				}
			}
			GUILayout.EndHorizontal();
			
			if (_movieCapture.IsCapturing())
			{
				if (!string.IsNullOrEmpty(_movieCapture.LastFilePath))
				{
					GUILayout.Label("Writing file: '" + System.IO.Path.GetFileName(_movieCapture.LastFilePath) + "'");
				}				
			}
			else
			{
				if (!string.IsNullOrEmpty(_movieCapture.LastFilePath))
				{
					GUILayout.Label("Last file written: '" + System.IO.Path.GetFileName(_movieCapture.LastFilePath) + "'");
				}				
			}
		}
		
		GUILayout.EndVertical();
	}

	void Update()
	{
		if (_whenRecordingAutoHideUI && !_showUI)
		{
			if (!_movieCapture.IsCapturing())
				_showUI = true;
		}
		
		if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F5))
		{
			if (_movieCapture.IsCapturing())
				_movieCapture.StopCapture();
		}
	}
}