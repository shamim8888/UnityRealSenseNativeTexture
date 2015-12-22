//--------------------------------------------------------------------------------------
// Copyright 2015 Intel Corporation
// All Rights Reserved
//
// Permission is granted to use, copy, distribute and prepare derivative works of this
// software for any purpose and without fee, provided, that the above copyright notice
// and this statement appear in all copies.  Intel makes no representations about the
// suitability of this software for any purpose.  THIS SOFTWARE IS PROVIDED "AS IS."
// INTEL SPECIFICALLY DISCLAIMS ALL WARRANTIES, EXPRESS OR IMPLIED, AND ALL LIABILITY,
// INCLUDING CONSEQUENTIAL AND OTHER INDIRECT DAMAGES, FOR THE USE OF THIS SOFTWARE,
// INCLUDING LIABILITY FOR INFRINGEMENT OF ANY PROPRIETARY RIGHTS, AND INCLUDING THE
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE.  Intel does not
// assume any responsibility for any errors which may appear in this software nor any
// responsibility to update it.
//--------------------------------------------------------------------------------------

using UnityEngine;

public class CameraRGBViewer : MonoBehaviour
{
	public SenseInput m_senseInput;
	private Texture2D m_texColor;
	private PXCMImage m_sample;
	private bool m_shuttingDown = false;
	private BlockProfiler mBlockProfiler = new BlockProfiler();

	private void Start()
	{
		m_senseInput.m_OnSample += OnSample;
		m_senseInput.m_ShutDown += OnShutdown;

		m_texColor = new Texture2D(m_senseInput.m_colorWidth, m_senseInput.m_colorHeight, TextureFormat.RGBA32, false);
		GetComponent<Renderer>().material.SetTexture("mainTex", m_texColor);
	}

	private void Update()
	{
		PXCMImage sample = null;
		lock (this)
		{
			if (m_sample == null) return;
			sample = m_sample;
			m_sample = null;
		}

		// display the color image
		PXCMImage.ImageData data;
		pxcmStatus sts = sample.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB32, out data);
		if (sts >= pxcmStatus.PXCM_STATUS_NO_ERROR)
		{
			mBlockProfiler.BeginBlock();
			data.ToTexture2D(0, m_texColor);
			mBlockProfiler.EndBlock();
			sample.ReleaseAccess(data);
			m_texColor.Apply();
		}

		sample.Dispose();
	}

	public float GetToTextureTime()
	{
		return mBlockProfiler.AverageSeconds;
	}

	private void OnSample(PXCMCapture.Sample sample)
	{
		if (m_shuttingDown) return;

		lock (this)
		{
			if (m_sample != null) m_sample.Dispose();
			m_sample = sample.color;
			m_sample.QueryInstance<PXCMAddRef>().AddRef();
		}
	}

	private void OnDisable()
	{
		lock (this)
		{
			if (m_sample != null)
			{
				m_sample.Dispose();
				m_sample = null;
			}
		}
	}

	private void OnShutdown()
	{
		m_shuttingDown = true;

		lock (this)
		{
			if (m_sample != null)
			{
				m_sample.Dispose();
				m_sample = null;
			}
		}
	}
}