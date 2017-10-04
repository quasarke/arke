# Arke for Asterisk ARI
Arke is an IVR written using .NET Core 2.0 for Asterisk using the ARI interface through AsterNET.ARI. It is built to be extensible and with easily modifiable call flows. It's cross platform, supporting Linux, Raspberry Pi, and Windows.

## Setup
* Clone the repo and run `dotnet restore` then `dotnet build`
* Setup the appsettings.json:
   * Set your asterisk host IP
   * Set the username and password for your Asterisk ARI Setup
   * Set the AsteriskAppName to the stasis app name you are using in your dialplan
   * Set your application to the name of the json file you wish to use for your call flow (you can copy arte.json for a starting point)
   * Set the Plugin Directory to the location you are going to copy all your plugins.
* Finally run `dotnet run --project Arke.ServiceHost`

## Dependencies
* AsterNET.ARI 1.2+
* .NET Core 2.0
* SimpleInjector 4
* NLog 5

## Extending and creating plugins for Arke
* Create a .NET Core Library with the namespace Arke.*
   * This allows Arke to auto-discover your Plugin without having to add the dependency
* Add references to the Arke.DSL and Arke.SipEngine projects or dlls.
* Settings should inherit the Arke.DSL.Step.Settings.ISettings interface
* Steps should inherit the Arke.SipEngine.Processors.IStepProcessor interface
* Compile and copy your new binary to the plugin directory (or use Visual Studio post-build scripts to do so for you)