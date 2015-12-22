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
using UnityEngine.UI;

public class GlobalSettings : MonoBehaviour
{
	public Toggle PluginToggle;
	public CameraRGBViewer rgbViewer;
	public CameraRGBViewerNative rgbViewerNative;
	public Text ToTextureTimeText;
	public Text FPSText;
	private BlockProfiler mFPSCounter = new BlockProfiler(15);

	public bool UsingNativePlugin { get { return PluginToggle.isOn; } }

	private void Start()
	{
		QualitySettings.vSyncCount = 0;
		PluginToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(PluginToggleChanged));
		PluginToggleChanged(PluginToggle.isOn);
	}

	private void PluginToggleChanged(bool value)
	{
		rgbViewer.gameObject.SetActive(!UsingNativePlugin);
		rgbViewerNative.gameObject.SetActive(UsingNativePlugin);
	}

	private void Update()
	{
		mFPSCounter.EndBlock();
		mFPSCounter.BeginBlock();

		float toTextureTime = UsingNativePlugin ? 0.0f : rgbViewer.GetToTextureTime();
		ToTextureTimeText.text = string.Format("<b>ToTexture2D: {0:0.00}ms</b>", toTextureTime * 1000.0f);

		float fps = (mFPSCounter.AverageSeconds > float.Epsilon) ? 1.0f / mFPSCounter.AverageSeconds : 0.0f;
		FPSText.text = string.Format("<b>FPS: {0:0.00}</b>", fps);
	}
}