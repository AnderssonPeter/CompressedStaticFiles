[![Build status](https://ci.appveyor.com/api/projects/status/na1crpmvw7huyy0l?svg=true)](https://ci.appveyor.com/project/AnderssonPeter/compressedstaticfiles)
[![NuGet version](https://badge.fury.io/nu/CompressedStaticFiles.svg)](https://badge.fury.io/nu/CompressedStaticFiles)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/AnderssonPeter/CompressedStaticFiles/master/LICENSE)


Ensure that you are using the `Kestrel` server without the IIS Integration.
Place `app.UseCompressedStaticFiles();` instead of `app.UseStaticFiles();` in `Startup.Configure()`.

This will ensure that you application will serve pre compressed `gzipped` `(filename.ext.gz)` and `brotli` `(filename.ext.br)` compressed files if the browser supports it.

Checkout the example for setup.

This solution is based on @neyromant from the following issue https://github.com/aspnet/Home/issues/1584#issuecomment-227455026.
