var gulp = require('gulp');
var msbuild = require('gulp-msbuild');
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


