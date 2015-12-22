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

public class SenseInput : MonoBehaviour
{
	public Text resolutionText;

	public int m_colorWidth = 320;
	public int m_colorHeight = 240;
	public int m_fps = 30;

	private bool hasInitFailed = false;
	private bool m_exitFlag = false;

	public delegate void OnSampleDelegate(PXCMCapture.Sample sample);
	public event OnSampleDelegate m_OnSample = null;

	public delegate void OnShutDownDelegate();
	public event OnShutDownDelegate m_ShutDown = null;

	private PXCMSenseManager m_senseManager = null;
	private pxcmStatus m_status = pxcmStatus.PXCM_STATUS_INIT_FAILED;

	public bool IsStreaming { get { return m_status >= pxcmStatus.PXCM_STATUS_NO_ERROR; } }

	private pxcmStatus OnNewSample(int mid, PXCMCapture.Sample sample)
	{
		if (m_OnSample != null)
		{
			m_OnSample(sample);
		}
		return pxcmStatus.PXCM_STATUS_NO_ERROR;
	}

	private void OnStatus(int mid, pxcmStatus sts)
	{
		// Camera failed or disconnected
		if (sts == pxcmStatus.PXCM_STATUS_ITEM_UNAVAILABLE || sts == pxcmStatus.PXCM_STATUS_EXEC_ABORTED)
		{
			m_exitFlag = true;
		}
	}

	private void InitializeStreaming()
	{
		PXCMSenseManager.Handler handler = new PXCMSenseManager.Handler();
		handler.onNewSample = OnNewSample;
		handler.onStatus = OnStatus;
		m_status = m_senseManager.Init(handler);
		if (m_status < pxcmStatus.PXCM_STATUS_NO_ERROR)
		{
			Debug.Log("Init Failed; " + m_status);
			hasInitFailed = true;
			return;
		}

		// Start streaming
		m_status = m_senseManager.StreamFrames(false);
		if (m_status < pxcmStatus.PXCM_STATUS_NO_ERROR)
		{
			Debug.Log("StreamFrames Failed; " + m_status);
			hasInitFailed = true;
		}
	}

	private void Start()
	{
		resolutionText.text = "Resolution: " + m_colorWidth + "x" + m_colorHeight;

		if (m_senseManager == null)
		{
			// Create a SenseManager instance
			m_senseManager = PXCMSenseManager.CreateInstance();
			if (m_senseManager == null)
			{
				Debug.Log("SenseManager Instance Failed");
				return;
			}

			// Enable color stream only
			PXCMVideoModule.DataDesc ddesc = new PXCMVideoModule.DataDesc();
			ddesc.streams.color.sizeMin.width = ddesc.streams.color.sizeMax.width = m_colorWidth;
			ddesc.streams.color.sizeMin.height = ddesc.streams.color.sizeMax.height = m_colorHeight;
			ddesc.streams.color.frameRate.min = ddesc.streams.color.frameRate.max = m_fps;
			m_senseManager.EnableStreams(ddesc);
		}
	}

	private void Update()
	{
		if (m_exitFlag)
		{
			Application.Quit();
		}
		// lazy initialize streaming. This ensure that the events don't start firing until all objects have
		// complete their Start initiaition.
		if (m_senseManager != null && !IsStreaming && !hasInitFailed)
		{
			InitializeStreaming();
		}

	}

	private void OnDisable()
	{
		if (m_ShutDown != null)
		{
			m_ShutDown();
		}

		if (m_senseManager != null)
		{
			m_senseManager.Close();
			m_senseManager.Dispose();
			m_senseManager = null;
		}
	}

	private void OnGUI()
	{
		if (!IsStreaming)
		{
			GUI.skin.box.hover.textColor =
				GUI.skin.box.normal.textColor =
					GUI.skin.box.active.textColor = Color.green;
			GUI.skin.box.alignment = TextAnchor.MiddleCenter;

			GUI.Box(new Rect(5, Screen.height - 35, 100, 30), "Setup Failed");
		}
	}
}