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

public class CameraRGBViewerNative : MonoBehaviour
{
	public SenseInput m_senseInput;

	private Texture2D m_texColor;
	private System.IntPtr m_texColorNative;

	private bool shuttingDown = false;

	private void Start()
	{
		// Get events when a new image comes in, or when RealSense is shutdown
		m_senseInput.m_OnSample += OnSampleCallback;
		m_senseInput.m_ShutDown += OnShutdownCallback;

		// Create a custom texture and bind it to the material
		m_texColor = new Texture2D(m_senseInput.m_colorWidth, m_senseInput.m_colorHeight, TextureFormat.RGBA32, false);
		m_texColorNative = m_texColor.GetNativeTexturePtr();
		Renderer renderer = GetComponent<Renderer>();
		renderer.material.SetTexture("mainTex", m_texColor);
	}

	private void OnSampleCallback(PXCMCapture.Sample sample)
	{
		if (shuttingDown) return;

		UseTexturePlugin.CopyPXCImageToTexture(sample.color, m_texColorNative);
	}

	private void OnShutdownCallback()
	{
		shuttingDown = true;
		UseTexturePlugin.RealSenseShutdown();
	}
}