IntelPerceptualSDK-WinformSample
================================

> Please note that this project usees the old Intel Perceptual SDK, while it still works, there is a new a [newer SDK](https://software.intel.com/en-us/intel-realsense-sdk) that looks very promising!

A Visual Studio C# .NET windows form sample for the Intel Perceptual Computing SDK 2013. 


### The project in a nutshell

Basically this is a program that uses Intel Perceptual SDK to perform face location/detection (including confidence), landmark detection (location of eyes, mouth, nose) and face attributes detection(age, emotion, eye status, gender) in a windows form. 

![](http://adelriosantiago.com/articles/images/facereco-test.png)

Feel free to use it on your projects. Remember you can contribute!


### Three easy steps to get the sample running on Visual Studio C# .NET

1.-Install the Intel Perceptual SDK from http://software.intel.com/en-us/vcsource/tools/perceptual-computing-sdk.

2.-Clone the repository on your desktop (click the "Clone in Desktop" button and follow the steps), and open de .sln file.

3.-Click run, if it fails then the reference to the library "libpxcclr" is incorrect. To fix it simply right-click on References folder on your Solution Explorer, click Add Reference, then Browse, then select the library located at the folder Resources.


### Contact information

You can find my old threads about Intel Perceptual [here](http://software.intel.com/en-us/user/815018).
