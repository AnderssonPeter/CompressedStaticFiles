/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    rename = require('gulp-rename'),
    brotli = require('gulp-brotli'),
    zopfli = require('gulp-zopfli')

var paths = ['wwwroot/**/*.js',
             'wwwroot/**/*.html',
             'wwwroot/**/*.htm',
             'wwwroot/**/*.css',
             'wwwroot/**/*.svg',
             'wwwroot/**/*.ico'];

gulp.task('gzip', function () {
    return gulp.src(paths)
               .pipe(zopfli())
               .pipe(gulp.dest('wwwroot'));
});

gulp.task('brotli', function () {
    return gulp.src(paths)
               .pipe(brotli.compress({ skipLarger: true, quality: 11 }))
               .pipe(gulp.dest('wwwroot'));
});