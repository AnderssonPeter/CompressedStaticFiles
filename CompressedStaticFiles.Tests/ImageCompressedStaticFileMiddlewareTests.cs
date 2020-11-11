#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressedStaticFiles.Tests
{
    [TestClass]
    public class ImageCompressedStaticFileMiddlewareTests
    {
        [TestMethod]
        public async Task GetSmallest()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/avif,image/webp");
            var response = await client.GetAsync("/IMG_6067.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/avif");
        }

        [TestMethod]
        public async Task FavIcon()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br, gzip");
            client.DefaultRequestHeaders.Add("Accept", "image/png,image/avif,image/webp");
            var response = await client.GetAsync("/favicon.ico");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/webp");
        }

        [TestMethod]
        public async Task GetSecondSmallest()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/webp");
            var response = await client.GetAsync("/IMG_6067.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/webp");
        }

        [TestMethod]
        public async Task ShouldNotHaveAcceptEncoding()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/webp");
            var response = await client.GetAsync("/IMG_6067.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.Contains("Content-Encoding").Should().BeFalse();
        }

        [TestMethod]
        public async Task GetWithoutSupport()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            var response = await client.GetAsync("/IMG_6067.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/jpeg");
        }

        [TestMethod]
        public async Task Disabled()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles(options => options.EnableImageSubstitution = false); ;
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/png,image/avif,image/webp");
            var response = await client.GetAsync("/IMG_6067.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/jpeg");
        }

        [TestMethod]
        public async Task PrioritizeSmallest()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles(options => options.RemoveImageSubstitutionCostRatio());
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/png,image/avif,image/webp");
            var response = await client.GetAsync("/highquality.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/jpeg");
        }

        [TestMethod]
        public async Task PrioritizeQualityAVIF()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/png,image/avif,image/webp");
            var response = await client.GetAsync("/highquality.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/avif");
        }

        [TestMethod]
        public async Task PrioritizeQualityWEBP()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles();
                    app.Use(next =>
                    {
                        return async context =>
                        {
                            // this test should never call the next middleware
                            // set status code to 999 to detect a test failure
                            context.Response.StatusCode = 999;
                        };
                    });
                }).UseWebRoot(Path.Combine(Environment.CurrentDirectory, "wwwroot"));
            var server = new TestServer(builder);

            // Act
            var client = server.CreateClient();
            client.DefaultRequestHeaders.Add("Accept", "image/png,image/webp");
            var response = await client.GetAsync("/highquality.jpg");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("image/webp");
        }
    }
}

