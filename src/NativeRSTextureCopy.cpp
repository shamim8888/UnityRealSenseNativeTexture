// Example low level rendering Unity plugin
#include "NativeRSTextureCopy.h"
#include "Unity/IUnityGraphics.h"
#include "pxcimage.h"

// --------------------------------------------------------------------------
// Include headers for the graphics APIs we support

#if SUPPORT_D3D9
	#include <d3d9.h>
	#include "Unity/IUnityGraphicsD3D9.h"
#endif
#if SUPPORT_D3D11
	#include <d3d11.h>
	#include "Unity/IUnityGraphicsD3D11.h"
#endif

#if SUPPORT_OPENGL
	#if UNITY_WIN || UNITY_LINUX
		#include <GL/gl.h>
	#else
		#include <OpenGL/gl.h>
	#endif
#endif

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

#if SUPPORT_D3D11
ID3D11Device *g_D3D11Device = NULL;
#endif

enum CRITSECT
{
	CRITSECT_COPYLIST,
	CRITSECT_COPYPROGRESS,
	CRITSECT_COUNT
};

#if UNITY_WIN
CRITICAL_SECTION gCriticalSections[CRITSECT_COUNT];
CRITICAL_SECTION gCopyCriticalSection;

void LockCriticalSection(CRITSECT section)
{
	EnterCriticalSection(&gCriticalSections[section]);
}

void UnlockCriticalSection(CRITSECT section)
{
	LeaveCriticalSection(&gCriticalSections[section]);
}
void Platform_Init()
{
	for (int i = 0; i < CRITSECT_COUNT; i++)
	{
		InitializeCriticalSection(&gCriticalSections[i]);
	}
}
#endif


// --------------------------------------------------------------------------
// Helper utilities


// Prints a string
static void DebugLog (const char* str)
{
	#if UNITY_WIN
	OutputDebugStringA (str);
	#else
	printf ("%s", str);
	#endif
}

// COM-like Release macro
#ifndef SAFE_RELEASE
#define SAFE_RELEASE(a) if (a) { a->Release(); a = NULL; }
#endif

struct SCopyInfo
{
	PXCImage *image;
	union
	{
		void *texture;
#if SUPPORT_D3D11
		ID3D11Texture2D *d3d11Teture;
#endif
#if SUPPORT_D3D9
		IDirect3DTexture9 *d3d9Teture;
#endif
	};
};

static void CopyInfoAddRef(SCopyInfo *info)
{
	info->image->AddRef();

#if SUPPORT_D3D11
	if (s_DeviceType == kUnityGfxRendererD3D11)
		info->d3d11Teture->AddRef();
#endif
}

static void CopyInfoRelease(SCopyInfo *info)
{
	info->image->Release();

#if SUPPORT_D3D11
	if (s_DeviceType == kUnityGfxRendererD3D11)
		info->d3d11Teture->Release();
#endif
}

static const int kMaxCopyQueue = 4;
SCopyInfo gCopyQueue[kMaxCopyQueue];
int gCopyQueueCount = 0;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API FlushQueuedCopies()
{
	LockCriticalSection(CRITSECT_COPYPROGRESS);
	LockCriticalSection(CRITSECT_COPYLIST);
	for (int i = 0; i < gCopyQueueCount; i++)
	{
		CopyInfoRelease(&gCopyQueue[i]);
	}
	gCopyQueueCount = 0;
	UnlockCriticalSection(CRITSECT_COPYLIST);
	UnlockCriticalSection(CRITSECT_COPYPROGRESS);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API CopyTextureData(void *srcPXCImage, void *dstTexture)
{
	if (srcPXCImage != NULL && dstTexture != NULL && s_DeviceType != kUnityGfxRendererNull)
	{
		SCopyInfo info;
		info.image = (PXCImage*)srcPXCImage;
		info.texture = dstTexture;
		
		CopyInfoAddRef(&info);

		LockCriticalSection(CRITSECT_COPYLIST);
		if (gCopyQueueCount >= kMaxCopyQueue)
		{
			CopyInfoRelease(&gCopyQueue[0]);
			for (int i = 1; i < kMaxCopyQueue; i++)
			{
				gCopyQueue[i - 1] = gCopyQueue[i];
			}
			gCopyQueueCount--;
		}
		gCopyQueue[gCopyQueueCount] = info;
		gCopyQueueCount++;
		UnlockCriticalSection(CRITSECT_COPYLIST);
	}
}

// --------------------------------------------------------------------------
// UnitySetInterfaces

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

extern "C" void	UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_UnityInterfaces = unityInterfaces;
	s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
	s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);
	
	// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
	OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
	s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// --------------------------------------------------------------------------
// GraphicsDeviceEvent

// Actual setup/teardown functions defined below
#if SUPPORT_D3D9
static void DoEventGraphicsDeviceD3D9(UnityGfxDeviceEventType eventType);
#endif
#if SUPPORT_D3D11
static void DoEventGraphicsDeviceD3D11(UnityGfxDeviceEventType eventType);
#endif

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
	UnityGfxRenderer currentDeviceType = s_DeviceType;

	switch (eventType)
	{
	case kUnityGfxDeviceEventInitialize:
		{
			DebugLog("OnGraphicsDeviceEvent(Initialize).\n");
			Platform_Init();
			s_DeviceType = s_Graphics->GetRenderer();
			currentDeviceType = s_DeviceType;
			break;
		}

	case kUnityGfxDeviceEventShutdown:
		{
			DebugLog("OnGraphicsDeviceEvent(Shutdown).\n");
			s_DeviceType = kUnityGfxRendererNull;
			break;
		}

	case kUnityGfxDeviceEventBeforeReset:
		{
			DebugLog("OnGraphicsDeviceEvent(BeforeReset).\n");
			break;
		}

	case kUnityGfxDeviceEventAfterReset:
		{
			DebugLog("OnGraphicsDeviceEvent(AfterReset).\n");
			break;
		}
	};

	#if SUPPORT_D3D9
	if (currentDeviceType == kUnityGfxRendererD3D9)
		DoEventGraphicsDeviceD3D9(eventType);
	#endif

	#if SUPPORT_D3D11
	if (currentDeviceType == kUnityGfxRendererD3D11)
		DoEventGraphicsDeviceD3D11(eventType);
	#endif
}

static void CopyImageToTextureInternal(SCopyInfo *info)
{

	if (s_DeviceType != kUnityGfxRendererD3D9 && s_DeviceType != kUnityGfxRendererD3D11 && s_DeviceType != kUnityGfxRendererOpenGL)
		return;

	PXCImage::ImageData colorImageData;

	if (info->image->AcquireAccess(PXCImage::ACCESS_READ, PXCImage::PixelFormat::PIXEL_FORMAT_RGB32, &colorImageData) != PXC_STATUS_NO_ERROR)
		return;

#if SUPPORT_D3D9
	// D3D9 case
	if (s_DeviceType == kUnityGfxRendererD3D9)
	{
		IDirect3DTexture9* d3dtex = (IDirect3DTexture9*)info->d3d9Teture;
		D3DSURFACE_DESC desc;
		d3dtex->GetLevelDesc(0, &desc);
		D3DLOCKED_RECT lr;
		
		if (SUCCEEDED(d3dtex->LockRect(0, &lr, NULL, D3DLOCK_DISCARD)))
		{
			if (lr.Pitch == colorImageData.pitches[0])
			{
				memcpy(lr.pBits, colorImageData.planes[0], desc.Height * lr.Pitch);
			}
			else
			{
				for (int y = 0; y < (int)desc.Height; y++)
				{
					byte *dst = (byte*)lr.pBits + y * lr.Pitch;
					byte *src = (byte*)colorImageData.planes[0] + y * colorImageData.pitches[0];
					memcpy(dst, src, lr.Pitch);
				}
			}
			d3dtex->UnlockRect(0);
		}
	}
#endif

#if SUPPORT_D3D11
	// D3D11 case
	if (s_DeviceType == kUnityGfxRendererD3D11 && g_D3D11Device != NULL)
	{
		ID3D11DeviceContext* ctx = NULL;
		g_D3D11Device->GetImmediateContext(&ctx);

		// update native texture from code
		ID3D11Texture2D* d3dtex = info->d3d11Teture;
		
		unsigned char* data = colorImageData.planes[0];
		ctx->UpdateSubresource(d3dtex, 0, NULL, data, colorImageData.pitches[0], 0);
		ctx->Release();
	}
#endif

#if SUPPORT_OPENGL
	// OpenGL case
	if (s_DeviceType == kUnityGfxRendererOpenGL)
	{
		GLuint gltex = (GLuint)(size_t)(info->texture);
		glBindTexture(GL_TEXTURE_2D, gltex);
		int texWidth, texHeight;
		glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &texWidth);
		glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_HEIGHT, &texHeight);

		if (texWidth == colorImageData.pitches[0]/4)
		{
			unsigned char* data = colorImageData.planes[0];
			glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, texWidth, texHeight, GL_RGBA, GL_UNSIGNED_BYTE, data);
		}
	}
#endif

	info->image->ReleaseAccess(&colorImageData);
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
	// Unknown graphics device type? Do nothing.
	if (s_DeviceType == kUnityGfxRendererNull)
		return;

	SCopyInfo toCopy[kMaxCopyQueue];
	int copyCount;

	LockCriticalSection(CRITSECT_COPYPROGRESS);
	LockCriticalSection(CRITSECT_COPYLIST);
	copyCount = gCopyQueueCount;
	memcpy(toCopy, gCopyQueue, sizeof(SCopyInfo) * gCopyQueueCount);
	gCopyQueueCount = 0;
	UnlockCriticalSection(CRITSECT_COPYLIST);

	for (int i = 0; i < copyCount; i++)
	{
		CopyImageToTextureInternal(&toCopy[i]);
		CopyInfoRelease(&toCopy[i]);
	}
	UnlockCriticalSection(CRITSECT_COPYPROGRESS);
}

// --------------------------------------------------------------------------
// GetRenderEventFunc, an example function we export which is used to get a rendering event callback function.
extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
	return OnRenderEvent;
}

// -------------------------------------------------------------------
//  Direct3D 9 setup/teardown code

#if SUPPORT_D3D9

static IDirect3DDevice9* g_D3D9Device;

// A dynamic vertex buffer just to demonstrate how to handle D3D9 device resets.
static IDirect3DVertexBuffer9* g_D3D9DynamicVB;

static void DoEventGraphicsDeviceD3D9(UnityGfxDeviceEventType eventType)
{
	// Create or release a small dynamic vertex buffer depending on the event type.
	switch (eventType) {
	case kUnityGfxDeviceEventInitialize:
		{
			IUnityGraphicsD3D9* d3d9 = s_UnityInterfaces->Get<IUnityGraphicsD3D9>();
			g_D3D9Device = d3d9->GetDevice();
		}
	case kUnityGfxDeviceEventAfterReset:
		// After device is initialized or was just reset, create the VB.
		if (!g_D3D9DynamicVB)
			g_D3D9Device->CreateVertexBuffer (1024, D3DUSAGE_WRITEONLY | D3DUSAGE_DYNAMIC, 0, D3DPOOL_DEFAULT, &g_D3D9DynamicVB, NULL);
		break;
	case kUnityGfxDeviceEventBeforeReset:
	case kUnityGfxDeviceEventShutdown:
		// Before device is reset or being shut down, release the VB.
		SAFE_RELEASE(g_D3D9DynamicVB);
		break;
	}
}

#endif // #if SUPPORT_D3D9

// -------------------------------------------------------------------
#if SUPPORT_D3D11
static void DoEventGraphicsDeviceD3D11(UnityGfxDeviceEventType eventType)
{
	if (eventType == kUnityGfxDeviceEventInitialize)
	{
		IUnityGraphicsD3D11* d3d11 = s_UnityInterfaces->Get<IUnityGraphicsD3D11>();
		g_D3D11Device = d3d11->GetDevice();
	}
}

#endif // #if SUPPORT_D3D11


