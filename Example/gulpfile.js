/// <binding ProjectOpened='watch' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    brotli = require('gulp-brotli'),
    zopfli = require('gulp-zopfli-green'),
    zlib = require('zlib')

var paths = ['wwwroot/**/*.js',
             'wwwroot/**/*.html',
             'wwwroot/**/*.htm',
             'wwwroot/**/*.css',
             'wwwroot/**/*.svg',
             'wwwroot/**/*.ico'];
var dest = 'wwwroot';

const copy = () =>
    gulp.src(['node_modules/bootstrap/dist/css/bootstrap.min.css',
        'node_modules/bootstrap/dist/css/bootstrap-grid.min.css',
        'node_modules/bootstrap/dist/css/bootstrap-reboot.min.css',
        'node_modules/bootstrap/dist/js/bootstrap.min.js'])
        .pipe(gulp.dest('wwwroot/'));

const compressGZip = () =>
    gulp.src(paths)
        .pipe(zopfli())
        .pipe(gulp.dest(dest));
const compressBrotli = () => 
    gulp.src(paths)
        .pipe(brotli({
            params: {
                [zlib.constants.BROTLI_PARAM_QUALITY]: zlib.constants.BROTLI_MAX_QUALITY,
            }
        }))
        .pipe(gulp.dest(dest));

exports.build = gulp.series(copy, gulp.parallel(compressGZip, compressBrotli));