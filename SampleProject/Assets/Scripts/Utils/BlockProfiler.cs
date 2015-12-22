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
using System.Diagnostics;

public class BlockProfiler
{
	private int[] mEntries;
	private int mIndex = 0;
	private int mEntryCount = 0;
	private long mSum = 0;
	private bool mInBlock = false;

	private Stopwatch mStopwatch = new Stopwatch();

	public BlockProfiler(int size = 128)
	{
		mEntries = new int[size];
	}

	public void BeginBlock()
	{
		mInBlock = true;
		mStopwatch.Start();
	}

	public float AverageMicroseconds { get { return (mEntryCount > 0) ? (mSum / mEntryCount) : 0; } }
	public float AverageMilliseconds { get { return AverageMicroseconds / 1000.0f; } }
	public float AverageSeconds { get { return AverageMicroseconds / 1000000.0f; } }

	public void EndBlock()
	{
		if (mInBlock)
		{
			mStopwatch.Stop();

			int elapsedUS = (int)(mStopwatch.Elapsed.TotalMilliseconds * 1000);

			mSum = mSum - mEntries[mIndex] + elapsedUS;
			mEntries[mIndex] = elapsedUS;
			mIndex = (mIndex + 1) % mEntries.Length;
			mEntryCount = Math.Min(mEntryCount + 1, mEntries.Length);

			mStopwatch.Reset();
			mInBlock = false;
		}
	}
}