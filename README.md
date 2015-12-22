Native Intel® RealSense™ SDK Image Copying in Unity
======================================================

This sample uses a native Unity plug-in to increase performance of displaying Intel® RealSense™ SDK image data by bypassing the C# layers of the SDK. Image data is uploaded to the GPU directly through the graphics API.

Build Instructions
==================
1. Download the Intel® RealSense™ SDK version R5 or greater
2. Run UpdateSDKDlls.bat in the SampleProject folder to copy the required SDK Dlls into the Unity project
3. Build the native plug-in – Open and build NativeRSTextureCopy.sln which can be found in within the src folder. A post-build step will copy the plug-in into the SampleProject folder.
4. Open and Run the SampleProject in Unity(not tested on versions lower than 5.2).

Requirements
============
- Windows 8.1 or Windows 10
- Visual Studio 2013 or higher with C++ compiler installed
- Intel® RealSense™ SDK (R5 release)
- Intel RealSense™ Camera*
- Unity version 5.2 or higher

The Intel® RealSense™ SDK can be download here:
https://software.intel.com/en-us/intel-realsense-sdk/download

For detailed information on this sample, please visit:
https://software.intel.com/en-us/articles/native-intel-realsense-sdk-image-copying-in-unity

 