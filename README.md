# Arke for Asterisk ARI

[![Join the chat at https://gitter.im/arkeivr/Lobby](https://badges.gitter.im/arkeivr/Lobby.svg)](https://gitter.im/arkeivr/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Arke is an IVR written using .NET Core 2.0 for Asterisk using the ARI interface through AsterNET.ARI. It is built to be extensible and with easily modifiable call flows. It's cross platform, supporting Linux, Raspberry Pi, and Windows.

## Find the developers
[![Join the chat at https://gitter.im/arkeivr/Lobby](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/arkeivr/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

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

## Features
* Playback sound files
* Get input with timeouts and fail / success steps
* DSL for building a call flow using JSON
* Extensible DSL for building and extending new steps into the system
* Plugin system for dynamically loading new steps at runtime
* Step Settings API for extending steps with extra settings in the DSL
* Async/Await Task based call processing for efficient mutli-threading and performance
* Stateless State Machine for call controls and validating step transitions
* Extensible Call State for holding data important to a call - can be used to build a custom CDR system
* Tested on Windows, Linux and Docker on both platforms.

## Known Issues
* Bridge Call Step still needs work
* Outbound calling hasn't been finalized yet. Needs some work on that as well
* No Steps for Voicemail and a few other basic IVR functions yet

## Planned Future Features
* More base steps to be included:
   * Voicemail
   * Call Transfer
* Play audio from web service
* Voice Conferencing creation and management
* Better outbound dialing support