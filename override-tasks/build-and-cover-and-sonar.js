var gulp = require('gulp');
var gutil = require('gulp-util');
var runSequence = require('run-sequence');

gulp.task('build-and-cover-and-sonar', function(cb) {
    runSequence('purge', 'git-submodules', 'build', 'sonar', function(err) {
        if (err) {
            gutil.log(gutil.colors.red(gutil.colors.bold(err)));
        }
        cb();
    });
});

