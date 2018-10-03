# RoleBot
This is the backend for a Discord Bot designed to manage revoke and assign roles based on reactions emotes to a certain message. Written in `C#` and using the `DSharp+` API, it's ready to be deployed on any device supporting the `.NET Core`.

## Getting Started
These instructions will get you a copy of the source code ready to check, edit and debug on your own machine. See deployment for notes on how to deploy on any system running the `.NET Runtime`.

### Prerequisites
To view the Solution, you will need either JetBrains Rider or Microsoft Visual Studio.

#### Using Rider:
Make sure to have the .NET Core SDK installed beforehand which can be done from `https://www.microsoft.com/net/learn/dotnet/hello-world-tutorial`, there are tutorials available for each separate platform.

#### Using Visual Studio:
When using Visual Studio, you will have to launch the `Visual Studio Installer` and get the `.NET Core Cross Platform Development` workload.
This will install the .NET SDK as well.

### Deployment
Deploying the solution is extremely simple throught the built-in packaging tools with either IDE else, you can use the command-line interface of the `.NET SDK` to achieve similar results.

#### Using an IDE:
When using an IDE to Package, you can simply right click on the project in the **Explorer** toolbar of either IDE and use the *Package* or *Publish* options.

#### Using .NET SDK CLI
After the installation of the .NET SDK, you can `cd` to the Directory of the `.sln` file and run `dotnet build`. This will place the project `.dll` file in the `<project source code>/bin/Debug/netcoreapp2.1/.`. If you want to build in a Release Configuration, use `dotnet build -c Release` in the `.sln` directory.