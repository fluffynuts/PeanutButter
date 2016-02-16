var gulp = require('gulp');
var child_process = require('child_process');
var nugetRestore = requireModule('gulp-nuget-restore');

gulp.task('nuget-restore', function(done) {
    return gulp.src('**/*.sln')
            .pipe(nugetRestore({
                debug: false,
                force: true
            }));
});
