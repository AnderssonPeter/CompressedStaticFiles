#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CompressedStaticFiles.Tests
{
    public class CompressedStaticFileMiddlewareTests
    {

        /// <summary>
        /// Call the next middleware if no matching file is found.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CallNextMiddleware()
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
                            context.Response.StatusCode = 999;
                        };
                    });
                });
            var server = new TestServer(builder);

            // Act
            var response = await server.CreateClient().GetAsync("/this_file_does_not_exist.html");

            // Assert
            response.StatusCode.Should().Be(999);
        }

        /// <summary>
        /// Serve the uncompressed file if no compressed version exist
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Uncompressed()
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
            var response = await server.CreateClient().GetAsync("/i_exist_only_uncompressed.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("uncompressed");
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("text/html");
        }

        /// <summary>
        /// Serve the compressed file if it exists and the browser supports it testing with a browser that supports both br and gzip
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SupportsBrAndGZip()
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
            var response = await client.GetAsync("/i_also_exist_compressed.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("br");
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("text/html");
        }

        /// <summary>
        /// Serve the compressed file if it exists and the browser supports it testing with a browser that only supports gzip
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task SupportsGzip()
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
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            var response = await client.GetAsync("/i_also_exist_compressed.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("gzip");
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("text/html");
        }

        /// <summary>
        /// Should send the uncompressed file if its smaller than the original
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UncompressedSmaller()
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
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br");
            var response = await client.GetAsync("/i_am_smaller_in_uncompressed.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("uncompressed");
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("text/html");
        }

        /// <summary>
        /// Use the FileProvider from options.
        /// </summary>
        [Fact]
        public async Task UseCustomFileProvider()
        {
            // Arrange
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.Exists.Returns(true);
            fileInfo.IsDirectory.Returns(false);
            fileInfo.Length.Returns(12);
            fileInfo.LastModified.Returns(new DateTimeOffset(2018, 12, 16, 13, 36, 0, new TimeSpan()));
            fileInfo.CreateReadStream().Returns(new MemoryStream(Encoding.UTF8.GetBytes("fileprovider")));

            var mockFileProvider = Substitute.For<IFileProvider>();
            mockFileProvider.GetFileInfo("/i_only_exist_in_mociFileProvider.html").Returns(fileInfo);

            var staticFileOptions = new StaticFileOptions() { FileProvider = mockFileProvider };

            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles();
                })
                .Configure(app =>
                {
                    app.UseCompressedStaticFiles(staticFileOptions);
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
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br");
            var response = await client.GetAsync("/i_only_exist_in_mociFileProvider.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("fileprovider");
            response.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues);
            contentTypeValues.Single().Should().Be("text/html");
        }

        /// <summary>
        /// Should not send precompressed content if it has been disabled.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Disabled()
        {
            // Arrange
            var builder = new WebHostBuilder()
                .ConfigureServices(sp =>
                {
                    sp.AddCompressedStaticFiles(options => options.EnablePrecompressedFiles = false);
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
            client.DefaultRequestHeaders.Add("Accept-Encoding", "br");
            var response = await client.GetAsync("/i_also_exist_compressed.html");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(200);
            content.Should().Be("uncompressed");
        }
    }
}

