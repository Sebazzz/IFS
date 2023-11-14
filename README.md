# Internet File Store

An ASP.NET Core application to host your own private small-scale upload system. You can upload your files and share them
with customers, friends, etc...

Built-in data retention. Secure. Fast.

## Building the project

To build the project ensure you have:

- .NET Core 2.2 SDK installed
- Node.js 8.0 or higher installed and in PATH
- Powershell 4 or higher

To build the project simply run:

    build

If you want to publish for a platform (win10-x64 for instance), run:

    build --target=Publish-Win10

To query for build targets:

    build --showdescription

## Deployment

To deploy the application, take the published files and install them under IIS or run the `IFS.Web` executable directly.
Ensure to give the application pool permissions to load its user profile.

In `appsettings.json` you can configure various settings, for instance where files are stored.

### Authentication

The application supports simple static authentication for upload and administration using a predefined password in the
configuration file,
or you can use OpenID Connect so users can login using authentication systems like AD FS.

For both authentication systems you can set a help text in the `LoginHelpText` setting.

#### Static authentication

There are two settings under the `/Authentication/Static` node:

- `Passphrase`: Used for uploads.
- `Administration`: `UserName` and `Password` for the administration console.

#### OpenID Connect authentication

Connect to an OpenID / OAuth provider like AD FS.

Settings under the `/Authentication/OpenIdConnect`
node: `ClientSecret`, `ClientId`, `MetadataAddress`, `Authority`, `Enable`.

In your OpenID server configure `<url>/oidc_sigin_admin` and `<url>/oidc_sigin_upload` as login urls.

Roles:

`RoleClaims`: For each role in the system (currently only `Administrator`), two keys:

- `ClaimType`: The type of claim sent by the OpenID server.
- `Value`: The contents of the claim to match and assign the given role to.

Additional claims:

`ClaimMapping`: To allow IFS to pre-fill some information you can enter the claims sent by the OpenID server.

- `Email`: Claim type for pre-filling the e-mail address.
- `Value`: Claim type for pre-filling the sender display name.

### Fail2Ban

The system can lock users out which attempt too many logins or enter too many wrong passwords in file downloads. This
currently works on IP address.

Settings under `Fail2Ban`:

- `DebounceTime`: A time string: the time after a IP-based lock-out is reset.
- `MaximumAttempts`: The number of password attempts until the ban is enforced for the duration of `DebounceTime`.

### Storage

Configure where and how the uploaded files are stored.

Settings under `FileStore`:

- `StorageDirectory`: The directory where the files and their metadata is placed.
- `MaximumFileSize`: The maximum uploadable file size in MB. If IFS is hosted behind IIS you can't set this higher then
  4096 MB. This is an IIS limitation.
