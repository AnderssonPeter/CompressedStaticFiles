[![NuGet version](https://badge.fury.io/nu/CompressedStaticFiles.svg)](https://badge.fury.io/nu/CompressedStaticFiles)
[![CompressedStaticFiles](https://circleci.com/gh/AnderssonPeter/CompressedStaticFiles.svg?style=svg)](https://circleci.com/gh/AnderssonPeter/CompressedStaticFiles)
[![GitHub license](https://img.shields.io/badge/license-Apache%202-blue.svg)](https://raw.githubusercontent.com/AnderssonPeter/CompressedStaticFiles/master/LICENSE)

<br />
<p align="center">
  <a href="https://github.com/AnderssonPeter/CompressedStaticFiles">
    <img src="icon.svg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">CompressedStaticFiles</h3>

  <p align="center">
    Serve smaller files with zero-ish server overhead
    <br />
    <a href="https://github.com/AnderssonPeter/CompressedStaticFiles/wiki"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    ·
    <a href="https://github.com/AnderssonPeter/CompressedStaticFiles/issues">Report Bug</a>
    ·
    <a href="https://github.com/AnderssonPeter/CompressedStaticFiles/issues">Request Feature</a>
  </p>
</p>

## Table of Contents
* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
* [Example](#example)
* [Acknowledgements](#acknowledgements)

## About The Project
This project allows you to server precompressed files to the browser without having to compress on demand, this is achieved by compressing/encoding your content at build time.

## Getting Started

### Precompress content
Static nonimage files have to be precompressed using [Zopfli](https://en.wikipedia.org/wiki/Zopfli) and/or [Brotli](https://en.wikipedia.org/wiki/Brotli), see the example for how to do it with gulp.
The files must have the exact same filename as the source + `.br` or `.gzip` (`index.html` would be `index.html.br` for the Brotli version).

### Encode images
Modern browsers support new image formates like webp and avif they can store more pixels per byte.
You can convert your images using the following tools [webp](https://developers.google.com/speed/webp/download) and [libavif](https://github.com/AOMediaCodec/libavif).
The files must have the same filename as the source but with a new file extension (`image.jpg` would be `image.webp` for the webp version).

### ASP.NET
Add `AddCompressedStaticFiles()` in your `Startup.ConfigureServices()` method.
Replace `UseStaticFiles();` with `UseCompressedStaticFiles();` in `Startup.Configure()`.
By default CompressedStaticFiles is configured to allow slightly larger files for some image formats as they can store more pixels per byte, this can be disabled by calling the entries from `CompressedStaticFileOptions.RemoveImageSubstitutionCostRatio()`.

## Example
A example can be found in the [Example](https://github.com/AnderssonPeter/CompressedStaticFiles/tree/master/Example) directory.
By using this package the Lighthouse mobile performance went from `76` to `98` and the transferred size went from `526 kb` to `141 kb`.

## Acknowledgements
This solution is based on @neyromant from the following issue https://github.com/aspnet/Home/issues/1584#issuecomment-227455026.
