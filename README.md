# FitNesseFitSharpUiAutomation

# Introduction 
This repo contains a fixture to enable automated testing of WPF/WinForms applications along with a number of demo FitNesse pages.
It only works on Windows since it uses Windows UI technology.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder `FitNesseFitSharpUiAutomation-master` into `%LOCALAPPDATA%\FitNesse`. 

* Go to the solution folder: `cd /D %LOCALAPPDATA%\FitNesse\UiAutomation`
* Build solution: `dotnet build --configuration release UiAutomation.sln`
* Go to fixture folder: `cd UiAutomation`
* Publish, including selecting the right runtime:<br/> `dotnet publish -o bin\Deploy\net5.0-windows -f net5.0-windows -c release -r win-x64 UiAutomation.csproj`
* Go to the assemby folder `bin\Deploy\net5.0-windows`.
* Edit `config.xml` and validate that it points to an existing `System.Windows.Forms.dll`. <br/>You can find out the right version via the command `dotnet --list-runtimes | find "Desktop.App 5"`
* Run FitNesse
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.UiAutomationSuite?suite

# Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
