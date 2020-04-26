[![NuGet version](https://badge.fury.io/nu/CompressedStaticFiles.svg)](https://badge.fury.io/nu/CompressedStaticFiles)
[![CompressedStaticFiles](https://circleci.com/gh/AnderssonPeter/CompressedStaticFiles.svg?style=svg)](https://circleci.com/gh/AnderssonPeter/CompressedStaticFiles)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/AnderssonPeter/CompressedStaticFiles/master/LICENSE)

This is a middleware for Asp</span>.Net Core 3.1 that provides a static files provider capable of providing pre-compressed static files.

This solution is based on @neyromant from the following issue https://github.com/aspnet/Home/issues/1584#issuecomment-227455026.

## Installation

You can install the latest stable version via [NuGet](https://www.nuget.org/packages/CompressedStaticFiles).

```
> dotnet add package CompressedStaticFiles
```

## Documentation

Ensure that you are using the `Kestrel` server without the IIS Integration or that the "Dynamic Content Compression" is disabled in IIS.

### Usage

Place `app.UseCompressedStaticFiles();` instead of `app.UseStaticFiles();` in `Startup.Configure()`.

This will ensure that your application will serve pre-compressed `gzip` `(filename.ext.gz)` or `brotli` `(filename.ext.br)` compressed files if the browser supports it. Without providing any options the default behavior is that, if the browser supports both `gzip` and `brotli` and if pre-compressed files for both types exist it will provide the smallest compressed files.

Checkout the `Example` project for usage.

### How to add more compression types

 Also if using `StaticFileOptions` remember to replace it with `CompressedStaticFileOptions`. `CompressedStaticFileOptions` inherits from `StaticFileOptions` and supports all the options that come with `StaticFileOptions`.

By default the middleware only supports `gzip` and `brotli`, to add more types you need to implement `ICompressionType` interface and replace `StaticFileOptions` with `CompressedStaticFileOptions` and then add the new compression type to the `CompressionTypes` list in `CompressedStaticFileOptions`.

> Note: the compression type needs to be supported in the commonly used browsers otherwise the users will not benefit from it.

here is an example of how to create a custom compression type:

```csharp
public class ExampleCompressionType: ICompressionType
{
    public string Encoding { get; } = "example";
    public string Extension { get; } = ".exm";
    public string ContentType { get; } = "application/example";
}
```

And here is how to set it up:

```csharp
var options = new CompressedStaticFileOptions();
options.CompressionTypes.Add<ExampleCompressionType>();
app.UsePreCompressedStaticFiles(options);
```
