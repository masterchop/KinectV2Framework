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
[AddComponentMenu("AVPro Movie Capture/From Camera (requires camera)")]
public class AVProMovieCaptureFromCamera : AVProMovieCaptureBase
{
	public bool _useFastPixelFormat = true;
	public Shader _shaderSwapRedBlue;
	public Shader _shaderRGBA2YCbCr;
	private Material _materialSwapRedBlue;
	private Material _materialRGBA2YCbCr;
	private Material _materialConversion;

	public override void Start()
	{
		_materialSwapRedBlue = new Material(_shaderSwapRedBlue);
		_materialSwapRedBlue.hideFlags = HideFlags.HideAndDontSave;
		_materialRGBA2YCbCr = new Material(_shaderRGBA2YCbCr);
		_materialRGBA2YCbCr.hideFlags = HideFlags.HideAndDontSave;
		_materialRGBA2YCbCr.SetFloat("flipY", 1.0f);

		base.Start();
	}

	private void OnRenderImage(RenderTexture source, RenderTexture dest)
	{
		if (_capturing && !_paused)
		{

			while (!AVProMovieCapturePlugin.IsNewFrameDue(_handle))
			{
				System.Threading.Thread.Sleep(1);
			}			
			//if (AVProMovieCapture.IsNewFrameDue(_handle))
			{
				
				RenderTexture buffer = RenderTexture.GetTemporary(_texture.width, _texture.height, 0);
				
				// Resize and convert pixel format
				// TODO perhaps we should pad instead of resizing to stop blurring due to resampling
				Graphics.Blit(source, buffer, _materialConversion);
				
				
				RenderTexture old = RenderTexture.active;
				RenderTexture.active = buffer;
				_texture.ReadPixels(new Rect(0, 0, buffer.width, buffer.height), 0, 0, false);
				
				EncodeTexture(_texture);
				RenderTexture.active = old;
				
				 
				RenderTexture.ReleaseTemporary(buffer);
				
				UpdateFPS();
				
			}
		}

		// Pass-through
		Graphics.Blit(source, dest);
	}

	public override void PrepareCapture()
	{
		if (_capturing)
			return;
		
		// Setup material
		_pixelFormat = AVProMovieCapturePlugin.PixelFormat.RGBA32;
		if (_useFastPixelFormat)
			_pixelFormat = AVProMovieCapturePlugin.PixelFormat.YCbCr422_YUY2;

		switch (_pixelFormat)
		{
			case AVProMovieCapturePlugin.PixelFormat.RGBA32:
				_materialConversion = _materialSwapRedBlue;
				_isTopDown = true;
				break;
			case AVProMovieCapturePlugin.PixelFormat.YCbCr422_YUY2:
				_materialConversion = _materialRGBA2YCbCr;
				_materialRGBA2YCbCr.SetFloat("flipY", 1.0f);
				_isTopDown = true;
				// If we're capturing uncompressed video in a YCbCr format we don't need to flip Y
				if (_codecIndex < 0)
				{
					_materialRGBA2YCbCr.SetFloat("flipY", 0.0f);
				}
				break;
		}
		if (_materialConversion == null)
		{
			Debug.LogError("Invalid pixel format");
			return;
		}

		SelectRecordingResolution(Screen.width, Screen.height);

		// When capturing YCbCr format we only need half the width texture
		int textureWidth = _targetWidth;
		if (_pixelFormat == AVProMovieCapturePlugin.PixelFormat.YCbCr422_YUY2)
			textureWidth /= 2;

		_texture = new Texture2D(textureWidth, _targetHeight, TextureFormat.ARGB32, false);

		GenerateFilename();

		base.PrepareCapture();
	}
}