using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2013 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

public class AVProMovieCaptureBase : MonoBehaviour 
{
	public enum FrameRate
	{
		Fifteen = 15,
		TwentyFour = 24,
		TwentyFive = 25,
		Thirty = 30,
		Sixty = 60,
	}
	
	public enum DownScale
	{
		Original = 1,
		Half = 2,
		Quarter = 4,
		Eighth = 8,
		Sixteenth = 16,
		Specific = 100,
	}	

	public KeyCode _captureKey = KeyCode.None;
	public bool _captureOnStart = false;
	public bool _listVideoCodecsOnStart = false;
	public string[] _videoCodecPriority = { "Lagarith Lossless Codec",
											"x264vfw - H.264/MPEG-4 AVC codec",
											"Xvid MPEG-4 Codec",
											"ffdshow video encoder",
											"Cinepak Codec by Radius",
											};
	public string[] _audioCodecPriority = { };
	public int _forceVideoCodecIndex = -1;
	public int _forceAudioCodecIndex = -1;
	public int _forceAudioDeviceIndex = -1;
	public FrameRate _frameRate = FrameRate.Thirty;
	public DownScale _downScale = DownScale.Original;
	public Vector2 _maxVideoSize = Vector2.zero;
	public bool _isRealTime = true;
	public bool _autoGenerateFilename = true;
	public string _autoFilenamePrefix = "MovieCapture";
	public string _forceFilename = "movie.avi";

	[System.NonSerialized]
	public string _codecName = "uncompressed";
	[System.NonSerialized]
	public int _codecIndex = -1;

	[System.NonSerialized]
	public string _audioCodecName = "uncompressed";
	[System.NonSerialized]
	public int _audioCodecIndex = -1;

	[System.NonSerialized]
	public string _audioDeviceName = "Unity";
	[System.NonSerialized]
	public int _audioDeviceIndex = -1;
	
	public bool _noAudio = false;

	public AVProUnityAudioCapture _audioCapture;

	protected Texture2D _texture;
	protected int _handle = -1;
	protected int _targetWidth, _targetHeight;
	protected bool _capturing = false;
	protected bool _paused = false;
	protected string _filePath;
	protected AVProMovieCapturePlugin.PixelFormat _pixelFormat = AVProMovieCapturePlugin.PixelFormat.YCbCr422_YUY2;
	private int _oldVSyncCount = 0;
	protected bool _isTopDown = true;
	protected bool _isDirectX11 = false;
	private bool _queuedStartCapture = false;
	
	public string LastFilePath  {
		get { return _filePath; }
	}
	
	public void Awake()
	{
		try
		{
			AVProMovieCapturePlugin.Init();
			Debug.Log("Using AVProMovieCapture plugin version: " + AVProMovieCapturePlugin.GetPluginVersion().ToString("F2"));
		}
		catch (DllNotFoundException e)
		{
			Debug.Log("Unity couldn't find the DLL, did you move the 'Plugins' folder to the root of your project?");
			throw e;
		}

		_isDirectX11 = SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11");
		
		SelectCodec(_listVideoCodecsOnStart);
		SelectAudioCodec(_listVideoCodecsOnStart);
		SelectAudioDevice(_listVideoCodecsOnStart);		
	}
	
	public virtual void Start() 
	{
		Application.runInBackground = true;
		
		if (_captureOnStart)
		{
			StartCapture();
		}
	}
	
	public void SelectCodec(bool listCodecs)
	{
		// Enumerate video codecs
		int numVideoCodecs = AVProMovieCapturePlugin.GetNumAVIVideoCodecs();
		if (listCodecs)
		{
			for (int i = 0; i < numVideoCodecs; i++)
			{
				Debug.Log("VideoCodec " + i + ": " + AVProMovieCapturePlugin.GetAVIVideoCodecName(i));
			}
		}
		
		// The user has specified their own codec index
		if (_forceVideoCodecIndex >= 0)
		{
			if (_forceVideoCodecIndex < numVideoCodecs)
			{
				_codecName = AVProMovieCapturePlugin.GetAVIVideoCodecName(_forceVideoCodecIndex);
				_codecIndex = _forceVideoCodecIndex;
			}
		}
		else
		{
			// Try to find the codec based on the priority list
			foreach (string codec in _videoCodecPriority)
			{
				string codecName = codec.Trim();
				// Empty string means uncompressed
				if (string.IsNullOrEmpty(codecName))
					break;
				
				for (int i = 0; i < numVideoCodecs; i++)
				{
					if (codecName == AVProMovieCapturePlugin.GetAVIVideoCodecName(i))
					{
						_codecName = codecName;
						_codecIndex = i;
						break;
					}
				}
				
				if (_codecIndex >= 0)
					break;
			}
		}
		
		if (_codecIndex < 0)
		{
			Debug.LogWarning("[AVProMovieCapture] Codec not found.  Video will be uncompressed.");
		}
	}
	

	public void SelectAudioCodec(bool listCodecs)
	{
		// Enumerate audio codecs
		int numAudioCodecs = AVProMovieCapturePlugin.GetNumAVIAudioCodecs();
		if (listCodecs)
		{
			for (int i = 0; i < numAudioCodecs; i++)
			{
				Debug.Log("AudioCodec " + i + ": " + AVProMovieCapturePlugin.GetAVIAudioCodecName(i));
			}
		}
		
		// The user has specified their own codec index
		if (_forceAudioCodecIndex >= 0)
		{
			if (_forceAudioCodecIndex < numAudioCodecs)
			{
				_audioCodecName = AVProMovieCapturePlugin.GetAVIAudioCodecName(_forceAudioCodecIndex);
				_audioCodecIndex = _forceAudioCodecIndex;
			}
		}
		else
		{
			// Try to find the codec based on the priority list
			foreach (string codec in _audioCodecPriority)
			{
				string codecName = codec.Trim();
				// Empty string means uncompressed
				if (string.IsNullOrEmpty(codecName))
					break;
				
				for (int i = 0; i < numAudioCodecs; i++)
				{
					if (codecName == AVProMovieCapturePlugin.GetAVIAudioCodecName(i))
					{
						_audioCodecName = codecName;
						_audioCodecIndex = i;
						break;
					}
				}
				
				if (_audioCodecIndex >= 0)
					break;
			}
		}
		
		if (_audioCodecIndex < 0)
		{
			Debug.LogWarning("[AVProMovieCapture] Codec not found.  Audio will be uncompressed.");
		}
	}	

	public void SelectAudioDevice(bool display)
	{
		// Enumerate
		int num = AVProMovieCapturePlugin.GetNumAVIAudioInputDevices();
		if (display)
		{
			for (int i = 0; i < num; i++)
			{
				Debug.Log("AudioDevice " + i + ": " + AVProMovieCapturePlugin.GetAVIAudioInputDeviceName(i));
			}
		}

		// The user has specified their own device index
		if (_forceAudioDeviceIndex >= 0)
		{
			if (_forceAudioDeviceIndex < num)
			{
				_audioDeviceName = AVProMovieCapturePlugin.GetAVIAudioInputDeviceName(_forceAudioDeviceIndex);
				_audioDeviceIndex = _forceAudioDeviceIndex;
			}
		}
		else
		{
			/*_audioDeviceIndex = -1;
			// Try to find one of the loopback devices
			for (int i = 0; i < num; i++)
			{
				StringBuilder sbName = new StringBuilder(512);
				if (AVProMovieCapturePlugin.GetAVIAudioInputDeviceName(i, sbName))
				{
					string[] loopbackNames = { "Stereo Mix", "What U Hear", "What You Hear", "Waveout Mix", "Mixed Output" };
					for (int j = 0; j < loopbackNames.Length; j++)
					{
						if (sbName.ToString().Contains(loopbackNames[j]))
						{
							_audioDeviceIndex = i;
							_audioDeviceName = sbName.ToString();
						}
					}
				}
				if (_audioDeviceIndex >= 0)
					break;
			}
			
			if (_audioDeviceIndex < 0)
			{
				// Resort to the no recording device
				_audioDeviceName = "Unity";
				_audioDeviceIndex = -1;
			}*/

			_audioDeviceName = "Unity";
			_audioDeviceIndex = -1;
		}
	}
	
	public void SelectRecordingResolution(int width, int height)
	{
		_targetWidth = width;
		_targetHeight = height;
		if (_downScale != DownScale.Specific)
		{
			_targetWidth /= (int)_downScale;
			_targetHeight /= (int)_downScale;
		}
		else
		{
			if (_maxVideoSize.x >= 1.0f && _maxVideoSize.y >= 1.0f)
			{
				_targetWidth = Mathf.FloorToInt(_maxVideoSize.x);
				_targetHeight = Mathf.FloorToInt(_maxVideoSize.y);
			}
		}
		
		// Some codecs like Lagarith in YUY2 mode need size to be multiple of 4
		_targetWidth = NextMultipleOf4(_targetWidth);
		_targetHeight = NextMultipleOf4(_targetHeight);
	}

	public void OnDestroy()
	{
		StopCapture();
		AVProMovieCapturePlugin.Deinit();
	}
	
	public void OnApplicationQuit()
	{
		StopCapture();
		AVProMovieCapturePlugin.Deinit();
	}
		
	protected void EncodeTexture(Texture2D texture)
	{
		Color32[] bytes = texture.GetPixels32();
		GCHandle _frameHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		
		if (_audioCapture == null || (_audioDeviceIndex >= 0 || _noAudio))
		{
			AVProMovieCapturePlugin.EncodeFrame(_handle, _frameHandle.AddrOfPinnedObject());
		}
		else
		{
			AVProMovieCapturePlugin.EncodeFrameWithAudio(_handle, _frameHandle.AddrOfPinnedObject(), _audioCapture.BufferPtr, (uint)_audioCapture.BufferLength);
			_audioCapture.FlushBuffer();
		}
		
		if (_frameHandle.IsAllocated)
			_frameHandle.Free();
	}
	
	public bool IsCapturing()
	{
		return _capturing;
	}

	public bool IsPaused()
	{
		return _paused;
	}
	
	public int GetRecordingWidth()
	{
		return _targetWidth;
	}
	
	public int GetRecordingHeight()
	{
		return _targetHeight;
	}
	
	protected void GenerateFilename()
	{
		TimeSpan span = (DateTime.Now - DateTime.Now.Date);
		string filename = _forceFilename;
		if (_autoGenerateFilename || string.IsNullOrEmpty(filename))
		{
			filename = Application.dataPath + "/../" + _autoFilenamePrefix;
			filename += "-" + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day;
			filename += "-" + ((int)(span.TotalSeconds)).ToString() + "s";
			filename += "-" + _targetWidth + "x" + _targetHeight + ".avi";
		}
		_filePath = filename;
		
		String directory = Path.GetDirectoryName(_filePath);
		if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			Directory.CreateDirectory(directory);
	}
	
	public virtual void PrepareCapture()
	{
		// Disable vsync
		if (QualitySettings.vSyncCount > 0)
		{
			_oldVSyncCount = QualitySettings.vSyncCount;
			//Debug.LogWarning("For best results vsync should be disabled during video capture.  Disabling vsync.");
			QualitySettings.vSyncCount = 0;
		}
		
		if (_isRealTime)
		{
			Application.targetFrameRate = (int)_frameRate;
		}
		else
		{
			Time.captureFramerate = (int)_frameRate;
		}
			
		Debug.Log("[AVProMovieCapture] New movie capture: (" + _targetWidth + "x" + _targetHeight + " @ " + ((int)_frameRate).ToString() + "fps) to file '" + _filePath + "'");
		Debug.Log("[AVProMovieCapture] Using video codec: '" + _codecName + "' audio device: '" + _audioDeviceName + "'");
		
		int audioDeviceIndex = _audioDeviceIndex;
		int audioCodecIndex = _audioCodecIndex;
		bool noAudio = _noAudio;
		if (_noAudio || (_audioCapture == null && _audioCodecIndex < 0))
		{
			audioCodecIndex = audioDeviceIndex = -1;
			noAudio = true;
		}

		_handle = AVProMovieCapturePlugin.CreateRecorderAVI(_filePath, (uint)_targetWidth, (uint)_targetHeight, (int)_frameRate,
													  (int)(_pixelFormat), _isTopDown, _codecIndex, !noAudio, audioDeviceIndex, audioCodecIndex, _isRealTime);
		
		if (_handle < 0)
		{
			Debug.LogWarning("[AVProMovieCapture] Failed to create recorder");
			StopCapture();
		}
	}
	
	public void QueueStartCapture()
	{
		_queuedStartCapture = true;
	}

	public void StartCapture()
	{
		if (_capturing)
			return;
				
		if (_handle < 0)
			PrepareCapture();

		if (_audioCapture && _audioDeviceIndex < 0 && !_noAudio)
		{
			_audioCapture.FlushBuffer();
			_audioCapture.enabled = true;
		}
		
		if (_handle >= 0)
		{
			AVProMovieCapturePlugin.Start(_handle);
			ResetFPS();
			_capturing = true;
			_paused = false;
		}
	}
	
	public void PauseCapture()
	{
		if (_capturing && !_paused)
		{
			AVProMovieCapturePlugin.Pause(_handle);
			_paused = true;
			ResetFPS();
		}
	}
	
	public void ResumeCapture()
	{
		if (_capturing && _paused)
		{
			AVProMovieCapturePlugin.Start(_handle);
			_paused = false;
		}
	}
	
	public void StopCapture()
	{
		if (_capturing)
		{
			Debug.Log("[AVProMovieCapture] Stopping capture");
			_capturing = false;
		}
		
		if (_audioCapture)
			_audioCapture.enabled = false;		
		
		Time.captureFramerate = 0;
		Application.targetFrameRate = -1;
		
		
		if (_oldVSyncCount > 0)
		{
			QualitySettings.vSyncCount = _oldVSyncCount;
			_oldVSyncCount = 0;
		}
		
		if (_handle >= 0)
		{
			AVProMovieCapturePlugin.Stop(_handle);
			System.Threading.Thread.Sleep(100);
			AVProMovieCapturePlugin.FreeRecorder(_handle);
			_handle = -1;
		}
		
		if (_texture != null)
		{
			Destroy(_texture);
			_texture = null;
		}
	}
	
	private void ToggleCapture()
	{
		if (_capturing)
			StopCapture();
		else
			StartCapture();
	}
	
	void Update() 
	{
		if (Input.GetKeyDown(_captureKey))
			ToggleCapture();
		
		if (_queuedStartCapture)
		{
			_queuedStartCapture = false;
			StartCapture();
		}
	}

	[NonSerializedAttribute]
	public float _fps;
	[NonSerializedAttribute]
	public int _frameTotal;
	
	private int _frameCount;
	private float _startFrameTime;
	
	protected void ResetFPS()
	{
		_frameCount = 0;
		_frameTotal = 0;
		_fps = 0.0f;
		_startFrameTime = 0.0f;
	}
	
	public void UpdateFPS()
	{
		_frameCount++;
		_frameTotal++;
		
		float timeNow = Time.realtimeSinceStartup;
		float timeDelta = timeNow - _startFrameTime;
		if (timeDelta >= 1.0f)
		{
			_fps = (float)_frameCount / timeDelta;
			_frameCount  = 0;
			_startFrameTime = timeNow;
		}
	}	
	
    private void ConfigureCodec() 
	{
		AVProMovieCapturePlugin.Init();
       	SelectCodec(false);
		if (_codecIndex >= 0)
		{
			AVProMovieCapturePlugin.ConfigureVideoCodec(_codecIndex);
		}
		//AVProMovieCapture.Deinit();
	}
	
	// Returns the next multiple of 4 or the same value if it's already a multiple of 4
	protected static int NextMultipleOf4(int value)
	{
		return (value + 3) & ~0x03;
	}	
}