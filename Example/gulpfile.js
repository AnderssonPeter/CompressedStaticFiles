/// <binding ProjectOpened='watch' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    brotli = require('gulp-brotli'),
    zopfli = require('gulp-zopfli-green'),
    zlib = require('zlib'),
    imagemin = require('gulp-imagemin'),
    imageminZopfli = require('imagemin-zopfli'),
    exec = require('gulp-exec');

var compressPaths =
    ['wwwroot/**/*.js',
     'wwwroot/**/*.html',
     'wwwroot/**/*.htm',
     'wwwroot/**/*.css',
     'wwwroot/**/*.svg',
     'wwwroot/**/*.ico'];

var dest = 'wwwroot';

const copy = () =>
    gulp.src(
        ['node_modules/bootstrap/dist/css/bootstrap.min.css',
         'node_modules/bootstrap/dist/css/bootstrap-grid.min.css',
         'node_modules/bootstrap/dist/css/bootstrap-reboot.min.css',
         'node_modules/bootstrap/dist/js/bootstrap.min.js',
         'images/*.*'])
        .pipe(gulp.dest('wwwroot/'));

const compressGZip = () =>
    gulp.src(compressPaths)
        .pipe(zopfli())
        .pipe(gulp.dest(dest));

const compressBrotli = () => 
    gulp.src(compressPaths)
        .pipe(brotli({
            params: {
                [zlib.constants.BROTLI_PARAM_QUALITY]: zlib.constants.BROTLI_MAX_QUALITY,
            }
        }))
        .pipe(gulp.dest(dest));

const compressZopfliPng = () => 
    gulp.src(['wwwroot/**/*.png'])
        .pipe(imagemin({
            use: [imageminZopfli()]
        }))
        .pipe(gulp.dest(dest));

var options = {
    continueOnError: false, // default = false, true means don't emit error event
    pipeStdout: false, // default = false, true means stdout is written to file.contents
};

var reportOptions = {
    err: true, // default = true, false means don't write err
    stderr: true, // default = true, false means don't write stderr
    stdout: true // default = true, false means don't write stdout
};

const compressPngToWebP = () =>
    gulp.src(['wwwroot/**/*.png'])
        .pipe(exec(file => `cwebp -lossless -exact -m 6 -z 9 ${file.path} -o ${file.path.substring(0, file.path.length - 3)}webp`, options))
        .pipe(exec.reporter(reportOptions));

//Skip Avif for png images as webp seems to always give smaller files.
const compressPngToAvif = () =>
    gulp.src(['wwwroot/**/*.png'])
        .pipe(exec(file => `avifenc --lossless -s 0 ${file.path} -o ${file.path.substring(0, file.path.length - 3)}avif`, options))
        .pipe(exec.reporter(reportOptions));

const compressPng = gulp.series(compressZopfliPng, compressPngToWebP);

const compressJpegToWebP = () =>
    gulp.src(['wwwroot/**/*.jpg'])
        .pipe(exec(file => `cwebp -preset photo -q 95 -m 6 ${file.path} -o ${file.path.substring(0, file.path.length - 3)}webp`, options))
        .pipe(exec.reporter(reportOptions));

const compressJpegToAvif = () =>
    gulp.src(['wwwroot/**/*.jpg'])
        .pipe(exec(file => `avifenc -s 0 --min 5 --max 15 ${file.path} -o ${file.path.substring(0, file.path.length - 3)}avif`, options))
        .pipe(exec.reporter(reportOptions));

const compressJpeg = gulp.series(compressJpegToWebP, compressJpegToAvif);

exports.build = gulp.series(copy, gulp.parallel(compressJpeg, compressGZip, compressBrotli, compressPng));
exports.buildWithOutImages = gulp.series(copy, gulp.parallel(compressGZip, compressBrotli));