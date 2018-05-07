# XDomain
A console program for scanning a file against the metadefender.opswat.com API

To replace the apikey, open the xml config file App.config inside the XDomain folder using any text editor and find the apikey entry in
appSettings, for example, <add key="apikey" value="c4b8b1cab7ef8132ae9dffce384b8975"/>, replace the value with your own key.
To change the pull interval for retrieving the scan results using the data_id, replace the value of the key "pollIntervalInMs" in the 
App.config file.

To build, rebuild, or clean the solution:
1. Create a local repo on a Windows 10 machine.
2. Open the file named Xdomain.sln using Visual Studio 2017. On the top menu bar, choose Build, select Build Solution, Rebuild Solution or 
Clean Solution from the dropdown menu. Or in Solution Explorer, right-click the solution and select Build Solution, Rebuild Solution or 
Clean Solution. All third-party library will be installed during the build process through NuGet. 
3. The default solution configuration is "Debug".

To execute the program:
1. After the build process is successfully completed, find the folder named "bin" inside the Xdomain folder, if the solution configuration 
is "Debug", find the "Debug" folder inside the bin folder. The executable is inside this folder. If the configuartion is "Release", find 
the executable in the "Release" folder, which is also in the bin folder.
2. The files (exe, dll, config files etc) in the Debug folder can be copied and used in different locations.  
3. For example, the program will be executed in the Debug folder. In the "Debug" folder, shift + right-click to open a powershell window 
inside the Debug folder, or open a powershell widow and cd to the Debug folder, 
4. The usage: .\Xdomain.exe upload_file file_path   Note: the first argument should always be "upload_file" (case insensetive), the second
argument should be the full path of the file, such as "‪D:\Xdomain\Xdomain\bin\Debug\test.pdf", unless the file is in the same folder with 
the executable.  



