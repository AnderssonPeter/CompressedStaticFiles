Ensure that you are using the `Kestrel` server without the IIS Integration.
Place `app.UseCompressedStaticFiles();` before `app.UseStaticFiles();` in `Startup.Configure()`.

This will ensure that you application will serve `gzipped` files and `brotli` compressed files if the browser supports it.
