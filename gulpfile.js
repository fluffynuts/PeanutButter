var gulp = require('gulp');
var msbuild = require('gulp-msbuild');
var fs = require('fs')
var nunit = require('gulp-nunit-runner');
var runSequence = require('run-sequence');
var dotCover = require('./gulp/gulp-dotcover');
var testUtilFinder = require('./gulp/testutil-finder');

gulp.task('test', function() {
    if (!fs.existsSync('buildreports')) {
        fs.mkdir('buildreports');
    }
    return gulp.src(['**/bin/Debug/**/*.Tests.dll'], { read: false })
                .pipe(nunit({
                    executable: testUtilFinder.latestNUnit(),
                    options: {
                        result: 'buildreports/nunit-result.xml'
                    }
                }));
});

gulp.task('build', function() {
    return gulp.src('**/*.sln')
            .pipe(msbuild({
                toolsVersion: 4.0,  // use 12.0 rather? 4.0 is supported by Mono...
                targets: ['Clean', 'Build'],
                configuration: 'Debug',
                stdout: true,
                verbosity: 'minimal',
                architecture: 'x86' // sqlite :/
            }));
});

gulp.task('clean', function() {
    return gulp.src('**/*.sln')
            .pipe(msbuild({
                toolsVersion: 4.0,
                targets: ['Clean'],
                configuration: 'Debug',
                stdout: true,
                verbosity: 'normal'
            }));
});

gulp.task('cover', function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(dotCover({
                 
             }));
});

gulp.task('default', function(cb) {
    runSequence('build', 'cover', function(err) {
        if (err) {
            console.log(err);
        }
        cb();
    });
});

