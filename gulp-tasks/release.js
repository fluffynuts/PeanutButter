var gulp = require('gulp');
var msbuild = require('gulp-msbuild');
gulp.task('release', function() {
    return gulp.src('**/*.sln')
            .pipe(msbuild({
                errorOnFail: true,
                toolsVersion: 14.0,  // use 12.0 rather? 4.0 is supported by Mono...
                targets: ['Clean', 'Build'],
                configuration: 'Release',
                stdout: true,
                verbosity: 'minimal',
                architecture: 'x64'
            }));
});


