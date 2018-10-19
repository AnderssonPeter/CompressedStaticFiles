/// <binding ProjectOpened='watch' />
/*
This file in the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. http://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp'),
    brotli = require('gulp-brotli'),
    zopfli = require('gulp-zopfli')

var paths = ['wwwroot/**/*.js',
             'wwwroot/**/*.html',
             'wwwroot/**/*.htm',
             'wwwroot/**/*.css',
             'wwwroot/**/*.svg',
             'wwwroot/**/*.ico'];
var dest = 'wwwroot';

gulp.task('copy', function () {
    gulp.src(['node_modules/bootstrap/dist/css/bootstrap.min.css',
        'node_modules/bootstrap/dist/css/bootstrap-grid.min.css',
        'node_modules/bootstrap/dist/css/bootstrap-reboot.min.css',
        'node_modules/bootstrap/dist/js/bootstrap.min.js'])
        .pipe(gulp.dest('wwwroot/'));
});

gulp.task('gzip', function () {
    return gulp.src(paths)
               .pipe(zopfli())
               .pipe(gulp.dest(dest));
});

gulp.task('brotli', function () {
    return gulp.src(paths)
               .pipe(brotli.compress({ quality: 11 }))
               .pipe(gulp.dest(dest));
});

gulp.task('build', ['gzip', 'brotli']);