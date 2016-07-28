Ensure that you are using the `Kestrel` server without the IIS Integration.
Open Startup.cs.
Add "using CompressedStaticFiles;".
Place "app.UseCompressedStaticFiles();" before "app.UseStaticFiles();" in Configure method.
This will ensure that you application will serve pre compressed gzipped (filename.ext.gz) and brotli (filename.ext.br) compressed files if the browser supports it.
Pre compress content (you can do this using gulp + gulp-brotli + gulp-zopfli or gulp-gzip.
Checkout the example if you need help!