using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2013 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[RequireComponent(typeof(Camera))]
[AddComponentMenu("AVPro Movie Capture/From Scene (requires camera)")]
public class AVProMovieCaptureFromScene : AVProMovieCaptureBase
{	
	public override void PrepareCapture()
	{
		if (_capturing)
			return;
				
		SelectRecordingResolution(Screen.width, Screen.height);

#if UNITY_3_5 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 
		if (_isDirectX11)
		{
			_texture = new Texture2D(_targetWidth, _targetHeight, TextureFormat.ARGB32, false);
		}
#else
		_texture = new Texture2D(_targetWidth, _targetHeight, TextureFormat.ARGB32, false);
#endif

		_pixelFormat = AVProMovieCapturePlugin.PixelFormat.RGBA32;
		if (SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL"))
		{
			_pixelFormat = AVProMovieCapturePlugin.PixelFormat.BGRA32;
			_isTopDown = true;
		}
		else
		{
			_isTopDown = false;
			if (_isDirectX11)
			{
				_isTopDown = true;	
			}
		}
		
		GenerateFilename();

		base.PrepareCapture();
	}
	
	private int _lastFrame;
	
	private IEnumerator FinalRenderCapture()
	{
		yield return new WaitForEndOfFrame();
		
		//System.Threading.Thread.Sleep(1000);
		while (!AVProMovieCapturePlugin.IsNewFrameDue(_handle))
		{
			System.Threading.Thread.Sleep(8);
		}
		
		/*int frame = Time.frameCount;
		if (frame - _lastFrame != 1)
		{
			Debug.Log("dropped: " + (frame - _lastFrame));
		}
		_lastFrame = frame;*/
		
		//System.Threading.Thread.Sleep(100);
		
		//if (IsNewFrameDue(_handle))
		{

		// Grab final RenderTexture into texture and encode
#if UNITY_3_5 || UNITY_4_1 || UNITY_4_0_1 || UNITY_4_0 

		if (!_isDirectX11)
		{
			if (_audioCapture && _audioDeviceIndex < 0 && !_noAudio)
			{
				AVProMovieCapturePlugin.EncodeAudio(_handle, _audioCapture.BufferPtr, (uint)_audioCapture.BufferLength);
				_audioCapture.FlushBuffer();
			}
			GL.IssuePluginEvent(AVProMovieCapturePlugin.PluginID | (int)AVProMovieCapturePlugin.PluginEvent.CaptureFrameBuffer | _handle);
		}
		else
		{
			_texture.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0, false);
			EncodeTexture(_texture);
		}
#else
		_texture.ReadPixels(new Rect(0, 0, _texture.width, _texture.height), 0, 0, false);
		EncodeTexture(_texture);
#endif
		UpdateFPS();
		}
	
		yield return null;
	}
	
	void OnPostRender()
	{
		if (_capturing && !_paused)
		{
			StartCoroutine("FinalRenderCapture");
		}
	}
}