var gulp = require('gulp');
var dotCover = require('./modules/gulp-dotcover');
gulp.task('cover', function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(dotCover({
                 debug: true,
                 architecture: 'x86'
             }));
});

