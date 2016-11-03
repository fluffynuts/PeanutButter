var
  gulp = require('gulp'),
  msbuild = require('gulp-msbuild');

// TODO: start from here and generate targets for all nuget packages
//        such that each package can be released individually and
//        conditionally on that package not existing yet
gulp.task('release-utils', ['nuget-restore'], function() {
    return gulp.src(['**/PeanutButter.Utils.NugetPackage.csproj', '!**/node_modules/**/*.sln'])
            .pipe(msbuild({
                toolsVersion: 14.0,
                targets: ['Clean', 'Build'],
                configuration: 'Release',
                stdout: true,
                verbosity: 'minimal',
                errorOnFail: true,
                architecture: 'x64'
            }));
});


