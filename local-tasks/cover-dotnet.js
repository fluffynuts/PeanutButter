var gulp = requireModule("gulp-with-help"),
    dotNetCover = requireModule("gulp-dotnetcover");
gulp.task("cover-dotnet", "Runs tests from projects matching *.Tests with DotCover coverage", function() {
    return gulp.src("**/*.Tests.dll")
             .pipe(dotNetCover({
                 debug: false,
                 architecture: "x86",
                 exclude: ["FluentMigrator.*",
                            "AutoMapper",
                            "AutoMapper.*",
                            "NUnit.*",
                            "nunit.*",
                            "Imported.*",
                            "GenericBuilderTest*",
                            "*.Tests.*"]
             }));
});
