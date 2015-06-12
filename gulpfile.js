var gulp = require('gulp');
var msbuild = require('gulp-msbuild');
var fs = require('fs')
var nunit = require('gulp-nunit-runner');
var runSequence = require('run-sequence');

var determineNUnitRunnerPath = function() {
    var baseFolder = 'C:/Program Files (x86)';
    var programFolders = fs.readdirSync(baseFolder);
    var nunitFolders = programFolders.reduce(function(acc, cur) {
        if (cur.toLowerCase().indexOf('nunit ') > -1) {
            acc.push(cur);
        }
        return acc;
    }, []);
    if (nunitFolders.length === 0) {
        throw 'Can\'t find NUnit under "C:/Program Files (x86)"';
    }
    nunitFolders.sort();
    var runner = [baseFolder, 
                    '/', 
                    nunitFolders[nunitFolders.length-1], 
                    '/bin/nunit-console.exe'].join('');
    if (!fs.existsSync(runner)) {
        throw 'Expected to find "' + runner + '" but didn\'t ):';
    }
    console.log(['Using test runner at: "', runner, '"'].join(''));
    return runner;
}

gulp.task('test', function() {
    return gulp.src(['**/bin/Debug/**/*.Tests.dll'], { read: false })
                .pipe(nunit({
                    executable: determineNUnitRunnerPath(),
                    result: 'buildreports/nunit-result.xml'
                }));
});

gulp.task('build', function() {
    return gulp.src('**/*.sln')
            .pipe(msbuild({
                toolsVersion: 4.0,  // use 12.0 rather? 4.0 is supported by Mono...
                targets: ['Clean', 'Build'],
                configuration: 'Debug',
                stdout: true,
                verbosity: 'minimal'
            }));
});

gulp.task('default', function(cb) {
    runSequence('build', 'test');
    cb();
});

