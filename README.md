# FitNesseFitSharpUiAutomation

# Introduction 
This repo contains a fixture to enable automated testing of WPF/WinForms applications along with a number of demo FitNesse pages.
It only works on Windows since it uses Windows UI technology.

# Installation
The steps to install are very similar to that of installing the [FibonacciDemo](../../../FitNesseFitSharpFibonacciDemo).

Differences are:
* Download the repo code as a zip file and extract the contents of the folder ```FitNesseFitSharpUiAutomation-master```. 
* Build command becomes: `dotnet build %LOCALAPPDATA%\FitNesse\UiAutomation\UiAutomation.sln`
* Go to folder: `cd /D %LOCALAPPDATA%\FitNesse\UiAutomation\UiAutomationTest\bin\debug\net5.0-windows`
* Before running FitNesse, edit `config.xml` and validate that it points to the right `System.Windows.Forms.dll`
* Run the suite: Open a browser and enter the URL http://localhost:8080/FitSharpDemos.UiAutomationSuite?suite

# Contribute
Enter an [issue](../../issues) or provide a [pull request](../../pulls). 
