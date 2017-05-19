var gulp = requireModule('gulp-with-help');
var coverDotNet = requireModule('gulp-dotnetcover');
gulp.task('cover-dotnet', "Covers all .NET tests", function() {
    return gulp.src('**/*.Tests.dll')
             .pipe(coverDotNet({
                 debug: false,
                 architecture: 'x86',
                 exclude: ['FluentMigrator.*', 
                            'AutoMapper',
                            'AutoMapper.*']
             }));
});

