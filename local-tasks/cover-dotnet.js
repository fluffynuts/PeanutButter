var gulp = requireModule('gulp-with-help');
var dotCover = requireModule('gulp-dotnetcover');
gulp.task('cover-dotnet', "Covers all .NET tests", [ "install-tools"], function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(dotCover({
                 debug: false,
                 architecture: 'x86',
                 exclude: ['FluentMigrator.*', 
                            'AutoMapper',
                            'AutoMapper.*']
             }));
});

