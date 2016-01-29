var gulp = require('gulp');
var dotCover = require('./modules/gulp-dotcover');
gulp.task('cover-dotnet', function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(dotCover({
                 debug: false,
                 architecture: 'x86',
                 exclude: ['FluentMigrator.*', 
                            'AutoMapper',
                            'AutoMapper.*',
                            'WindsorTestHelpers.*',
                            'MvcTestHelpers',
                            'TestUtils']
             }));
});

