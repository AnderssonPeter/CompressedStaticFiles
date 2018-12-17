#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.FileProviders;
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
    public class CompressedStaticFileMiddlewareTests
    {

        [TestMethod]
        public async Task Should_call_next_middleware_if_file_is_not_found()
        {
            // Arrange
            var builder = new WebHostBuilder()
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

        [TestMethod]
        public async Task Should_serve_the_uncompressed_file_if_a_compressed_version_does_not_exist()
        {
            // Arrange
            var builder = new WebHostBuilder()
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

        [TestMethod]
        public async Task Should_serve_the_compressed_file_if_a_compressed_version_exists_and_the_browser_supports_it()
        {
            // Arrange
            var builder = new WebHostBuilder()
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

        [TestMethod]
        public async Task Should_serve_gzip_and_not_br_if_only_gzip_is_accepted_by_the_browser()
        {
            // Arrange
            var builder = new WebHostBuilder()
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


        [TestMethod]
        public async Task Should_serve_the_uncompressed_file_if_it_is_smaller()
        {
            // Arrange
            var builder = new WebHostBuilder()
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

        [TestMethod]
        public async Task Should_use_the_file_provider_of_StaticFileOptions_if_it_is_provided()
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
    }
}

