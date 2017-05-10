const
  gulp = requireModule("gulp-with-help"),
  installLocalTools = requireModule("install-local-tools");

gulp.task("install-tools", () => {
  return installLocalTools([
    "nunit.console",
    "opencover",
    "reportgenerator"
  ]);
});