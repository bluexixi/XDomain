## XDomain
A console program for scanning a file against the metadefender.opswat.com API.

This program is wrote in .NET 4.7.1 using C# and contains four classes, CLi, InputFile, ApiAccessUtil and JsonHelper. CLi contains the 
program entry. InputFile is a warpper class for the metadata of the file, which is in the Entity folder. The binary data of the file is 
only accessed through file IO when needed. The ApiAccessUtil is a static class containing method for calling API and the JsonHelper is 
another static class for parsing JSON data. Both classes are in the Util folder. The NLog library is used to log information and Json.NET 
library is used for parsing JSON. 

To replace the apikey, open the xml config file **App.config** inside the XDomain folder using any text editor and find the apikey entry 
in appSettings, for example, `<add key="apikey" value="c4b8b1cab7ef8132ae9dffce384b8975"/>`, replace the value with your own key.

To change the pull interval for retrieving the scan results using the data_id, replace the value of the key "pollIntervalInMs" in the 
**App.config** file.

To build, rebuild, or clean the solution:
1. Create a local repo on a Windows 10 machine.
2. Open the file named **Xdomain.sln** using Visual Studio 2017. On the top menu bar, choose Build, select Build Solution, Rebuild 
Solution or Clean Solution from the dropdown menu. Or in Solution Explorer, right-click the solution and select Build Solution, Rebuild 
Solution or Clean Solution. All third-party libraries will be installed during the build process through NuGet. The default solution 
configuration is "Debug". 

To execute the program:
1. After the build process is successfully completed, find the folder named **bin** inside the **Xdomain** folder, if the solution configuration is "Debug", find the **Debug** folder inside the bin folder. The executable is inside the folder. If the configuartion is "Release", find the executable in the **Release** folder, which is also in the bin folder.
2. The files (exe, dll, config files etc) in the Debug folder can be copied and executed in other locations.  
3. For example, the program will be executed in the Debug folder. In the "Debug" folder, shift + right-click to open a powershell window 
inside the Debug folder, or open a powershell widow and cd to the Debug folder, 
4. The only supported command usage: **.\Xdomain.exe upload_file file_path**   Note: the first argument should always be "upload_file" 
(case insensetive), the second argument should be the full path of the file, such as `D:\Files\test.pdf`, unless the file is in the same 
folder with the executable.  



