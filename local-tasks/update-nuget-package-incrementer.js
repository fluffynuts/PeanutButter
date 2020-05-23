const gulp = requireModule("gulp-with-help"),
  runSequence = requireModule("run-sequence"),
  throwIfNoFiles = requireModule("throw-if-no-files"),
  del = require("del"),
  { clean, publish } = require("gulp-dotnet-cli");

gulp.task("purge-nuget-package-incrementer", () => {
  return del([
    "source/NugetPackageVersionIncrementer/obj",
    "source/NugetPackageVersionIncrementer/bin"
  ]);
});

gulp.task("purge utils/bin", () => {
  return del(["utils/bin"]);
});

gulp.task(
  "build-nuget-package-incrementer",
  ["purge-nuget-package-incrementer", "purge utils/bin"],
  () => {
    return gulp.src("**/NugetPackageVersionIncrementer.csproj")
    .pipe(throwIfNoFiles("NugetPackageVersionIncrementer.csproj not found"))
    .pipe(
      publish({
        configuration: "Release",
        framework: "netcoreapp2.2",
        runtime: "win10-x64"
      })
    );
  }
);

gulp.task(
  "update-nuget-package-incrementer",
  ["build-nuget-package-incrementer"],
  () => {
    return gulp
      .src("*", {
        cwd:
          "source/NugetPackageVersionIncrementer/bin/Release/netcoreapp3.0/win10-x64"
      })
      .pipe(throwIfNoFiles("No build artifacts for NugetPackageIncrementer, framework netcoreapp2.2, runtime win10-x64"))
      .pipe(gulp.dest("utils/bin"));
  }
);

// the fancy way that NugetPackageVersionIncrementer is built
// (net452 for debug and netcoreapp for Release), confuses
// msbuild... let's help the little critter out!
gulp.task("prebuild", ["purge-nuget-package-incrementer"], done => {
  runSequence("nuget-restore", done);
});
