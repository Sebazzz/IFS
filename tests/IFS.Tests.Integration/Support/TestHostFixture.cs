// ******************************************************************************
//  © 2016 Sebastiaan Dammann - damsteen.nl
// 
//  File:           : TestHostFixture.cs
//  Project         : IFS.Tests.Integration
// ******************************************************************************

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace IFS.Tests.Integration.Support;

internal abstract class TestHostFixture : PageTest, IDisposable
{
    private IHostBuilder _hostBuilder;
    private IHost _host;
    private string _hostInitializationStack;

    protected IHostBuilder HostBuilder
    {
        get
        {
            if (this._host is not null)
                throw new InvalidOperationException(
                    $"Too late - the host was already initialized at: {this._hostInitializationStack}");
            return this._hostBuilder;
        }
    }

    protected IHost Host
    {
        get
        {
            if (this._host is null)
            {
                this._host = this._hostBuilder.Build();
                this._hostInitializationStack = Environment.StackTrace;
                this._host.Start();
            }

            return this._host;
        }
    }

    private IServer Server => this.Services.GetRequiredService<IServer>();

    protected Uri BaseUri => new Uri(this.Server.Features.Get<IServerAddressesFeature>().Addresses.First());

    protected IServiceProvider Services => this.Host.Services;

    [OneTimeSetUp]
    public void SetUpHost()
    {
        this._hostBuilder = TestHostFactory.MakeHostBuilder();
    }

    [TearDown]
    public async Task TryCreateScreenshot()
    {
        try
        {
            var screenshotPath = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                $"end-of-test-{TestContext.CurrentContext.Test.Name}.png");

            await TestContext.Progress.WriteLineAsync($"Writing end of test screenshot to {screenshotPath}...");

            await File.WriteAllBytesAsync(
                screenshotPath,
                await this.Page.ScreenshotAsync()
            );

            await TestContext.Progress.WriteLineAsync($"... wrote end of test screenshot to {screenshotPath}");

            TestContext.AddTestAttachment(screenshotPath, "End of test screenshot");
        }
        catch (Exception ex)
        {
            await TestContext.Progress.WriteLineAsync($"Unable to create end-of-test screenshot: {ex}");
        }
    }

    [OneTimeTearDown]
    public void TearDownHost()
    {
        this._host?.Dispose();
        this._host = null;
        this._hostBuilder = null;
    }

    protected string MakeUrl(string rootedPath)
    {
        return new Uri(this.BaseUri, rootedPath).ToString();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._host?.Dispose();
            this._host = null;
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}