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

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class UseTexturePlugin : MonoBehaviour
{

	[DllImport("NativeRSTextureCopy")]
	private static extern void CopyTextureData(System.IntPtr srcPXCImage, System.IntPtr dstTexture);

	[DllImport("NativeRSTextureCopy")]
	private static extern IntPtr GetRenderEventFunc();

	[DllImport("NativeRSTextureCopy")]
	private static extern void FlushQueuedCopies();

	public static void CopyPXCImageToTexture(PXCMImage srcImage, System.IntPtr dstTexture)
	{
		if (srcImage != null && dstTexture != System.IntPtr.Zero )
		{
			CopyTextureData(srcImage.QueryNativePointer(), dstTexture);
		}
	}

	private IEnumerator Start()
	{
		yield return StartCoroutine("CallPluginAtEndOfFrames");
	}

	private IEnumerator CallPluginAtEndOfFrames()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			GL.IssuePluginEvent(GetRenderEventFunc(), 1);
		}
	}

	public static void RealSenseShutdown()
	{
		FlushQueuedCopies();
	}
}