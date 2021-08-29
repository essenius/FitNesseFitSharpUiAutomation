# FitNesseFitSharpUiAutomation

# Introduction 
This repo contains a fixture to enable automated testing of WPF/WinForms applications along with a number of demo FitNesse pages.
It only works on Windows since it uses Windows UI technology.

# Getting Started
1. Download FitNesse (http://fitnesse.org) and install it to <I>C:\Apps\FitNesse</I>
2. Install the FitSharp NuGet package with target output directory specified:
   ```
   nuget install FitSharp -OutputDirectory C:\Apps
   ```
   if you prefer not having version number as part of package folder name, use <I>-ExcludeVersion</I> switch to exclude the version number.
   
   ```
   nuget install FitSharp -OutputDirectory C:\Apps -ExcludeVersion
   ```
   After executing above command, FitSharp will be installed to <I>C:\Apps\FitSharp</I>.
   
3. Clone the repo to a local folder, e.g. <I>C:\Data\FitNesseDemo</I>. This is the folder where plugins.properties should be.
```
mkdir c:\data\FitNesseDemo
cd /d c:\data\FitNesseDemo
git clone  https://github.com/essenius/FitNesseFitSharpUiAUtomation .
```
4. Update <I>plugins.properties</I> to point to the FitSharp folder (if you took other folders than suggested)
5. Build all projects in the solution UiAutomation in Visual Studio Code:
   ```
   dotnet build UiAutomation.sln -c release
   ```
6. Ensure you have Java installed (1.7 or higher)
7. Start FitNesse with the root repo folder as the data folder, and the test assembly folder as the current directory:

	```
	cd /D C:\Data\FitNesseDemo\UiAutomation\UiAutomationTest\bin\Release\net5.0-windows

	java -jar C:\Apps\FitNesse\fitnesse-standalone.jar -d c:\data\FitNesseDemo -e 0
	```
    
8. Open a browser and enter the URL http://localhost:8080/FitSharpDemos.UiAutomationDemoSuite?suite

# Contribute
Enter an issue or provide a pull request.
