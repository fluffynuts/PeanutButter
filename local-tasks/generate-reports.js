var gulp = requireModule("gulp-with-help"),
  findTool = requireModule("testutil-finder").findTool,
  exec = requireModule("exec"),
  fs = require("fs"),
  path = require("path");

function findCoverageXml() {
  var check = path.join("buildreports", "coverage.xml");
  return fs.existsSync(check) ? check : null;
}

gulp.task(
  "generate-reports",
  "Generates HTML reports from existing coverage XML reports",
  () => {
    var reportGenerator = findTool("ReportGenerator.exe");
    if (!reportGenerator) {
      return Promise.reject("No ReportGenerator.exe found in tools folder");
    }
    var coverageXml = findCoverageXml();
    if (!coverageXml) {
      return Promise.reject("Can't find coverage.xml");
    }
    return exec(
      reportGenerator,
      [
        `--reports:${coverageXml}`,
        `--targetdir:${path.join("buildreports", "coverage")}`
      ]
    );
});
  