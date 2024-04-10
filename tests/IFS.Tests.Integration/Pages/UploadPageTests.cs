// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : UploadPageTests.cs
//  Project         : IFS.Tests.Integration
// ******************************************************************************

using System.Threading.Tasks;
using IFS.Tests.Integration.Support;
using IFS.Web.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace IFS.Tests.Integration.Pages;

[TestFixture]
internal sealed class UploadPageTests : TestHostFixture
{
    private string _loginHelpText;

    [OneTimeSetUp]
    public void SetSettings()
    {
        this._loginHelpText = TestContext.CurrentContext.Random.GetString(256);
        this.HostBuilder.ConfigureServices(services =>
            services.Configure<AuthenticationOptions>(auth => auth.LoginHelpText = this._loginHelpText));
    }

    [Test]
    public async Task HomePage_HasLoginScreenDisplayed()
    {
        // When
        await this.Page.GotoAsync(this.MakeUrl("/"));

        // Then
        await this.Expect(this.Page).ToHaveURLAsync(this.MakeUrl("/authenticate/login?returnUrl=%2Fupload"));
        await this.Expect(this.Page.GetByTestId("help-text")).ToContainTextAsync(this._loginHelpText);
    }
}