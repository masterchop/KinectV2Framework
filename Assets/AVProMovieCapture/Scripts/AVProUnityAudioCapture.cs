using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

//-----------------------------------------------------------------------------
// Copyright 2012-2013 RenderHeads Ltd.  All rights reserved.
//-----------------------------------------------------------------------------

[RequireComponent(typeof(AudioListener))]
[AddComponentMenu("AVPro Movie Capture/Audio Capture (requires AudioListener)")]
public class AVProUnityAudioCapture : MonoBehaviour 
{
	private float[] _buffer;
	private int _bufferIndex;
	private GCHandle _bufferHandle;
	
	public float[] Buffer  { get { return _buffer; } }
	public int BufferLength  { get { return _bufferIndex; } }
	public System.IntPtr BufferPtr { get { return  _bufferHandle.AddrOfPinnedObject(); } }
	
	void OnEnable()
	{
		Debug.Log("SampleRate: " + AudioSettings.outputSampleRate);
		Debug.Log("Speaker: " + AudioSettings.speakerMode.ToString());
		int bufferLength = 0;
		int numBuffers = 0;
		AudioSettings.GetDSPBufferSize(out bufferLength, out numBuffers);
		Debug.Log("DSP using " + numBuffers + " buffers of " + bufferLength + " bytes");
		
		_buffer = new float[bufferLength*256];
		_bufferIndex = 0;
		Debug.Log("Buffer size: " + _buffer.Length);
		
		_bufferHandle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
	}
	
	void OnDisable()
	{
		FlushBuffer();
		
		if (_bufferHandle.IsAllocated)
			_bufferHandle.Free();
	}
	
	public void FlushBuffer()
	{
		_bufferIndex = 0;
	}

	void OnAudioFilterRead(float[] data, int channels)
	{
		int length = Mathf.Min(data.Length, _buffer.Length - _bufferIndex);
		
		//System.Array.Copy(data, 0, _buffer, _bufferIndex, length);
    	for (int i = 0; i < length; i++)
		{
        	_buffer[i + _bufferIndex] = data[i];
		}
		_bufferIndex += length;
	}
}