# Internet File Store
An ASP.NET Core application to host your own private small-scale upload system. You can upload your files and share them with customers, friends, etc... 

Built-in data retention. Secure. Fast.

## Building the project
To build the project ensure you have:

- .NET Core 2.1 SDK installed
- Node.js 6.0 or higher installed and in PATH
- Powershell 4 or higher

To build the project simply run:

    build

If you want to publish for a platform (win10-x64 for instance), run:

    build -Target Publish-Win10

## Deployment
To deploy the application, take the published files and install them under IIS or run the `IFS.Web` executable directly. 

In `appsettings.json` you can configure various settings, for instance where files are stored.