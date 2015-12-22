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
#pragma once

#include "Unity/IUnityInterface.h"


// Which platform we are on?
#if _MSC_VER
#define UNITY_WIN 1
#elif defined(__APPLE__)
	#if defined(__arm__)
		#define UNITY_IPHONE 1
	#else
		#define UNITY_OSX 1
	#endif
#elif defined(__linux__)
#define UNITY_LINUX 1
#elif defined(UNITY_METRO) || defined(UNITY_ANDROID)
// these are defined externally
#else
#error "Unknown platform!"
#endif



// Which graphics device APIs we possibly support?$(SolutionDir)..\..\UnityProject\Assets\Plugins\x86_64
#if UNITY_METRO
	#define SUPPORT_D3D11 1
#elif UNITY_WIN
	#define SUPPORT_D3D9 1
	#define SUPPORT_D3D11 1 // comment this out if you don't have D3D11 header/library files
	#ifdef _MSC_VER
	  #if _MSC_VER >= 1900
	    //#define SUPPORT_D3D12 1
	  #endif
	#endif
	#define SUPPORT_OPENGL 1
#elif UNITY_IPHONE || UNITY_ANDROID
	#define SUPPORT_OPENGLES 1
#elif UNITY_OSX || UNITY_LINUX
	#define SUPPORT_OPENGL 1
#endif
