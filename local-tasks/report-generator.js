const
  gulp = requireModule("gulp-with-help"),
  path = require("path"),
  findTool = requireModule("testutil-finder").findTool,
  exec = requireModule("exec"),
  fs = require("fs"),
  defaultExclusions = "-" + [
    "*.Tests",
    "FluentMigrator",
    "FluentMigrator.Runner",
    "GenericBuilderTestArtifactBuilders",
    "GenericBuilderTestArtifactEntities"
  ].join(";-"),
  defaultReportsPath = path.join("buildreports", "coverage.xml"),
  buildReportsPath = process.env.COVERAGE_XML || defaultReportsPath,
  coverageExclude = process.env.COVERAGE_EXCLUDE || defaultExclusions;

function findCoverageXml() {
  return fs.existsSync(buildReportsPath) ? buildReportsPath : null;
}

gulp.task(
  "report-generator",
  `Generates HTML reports from existing coverage XML reports at ${buildReportsPath}`,
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
        `-reports:${coverageXml}`,
        `-targetdir:${path.join("buildreports", "coverage")}`,
        `"-assemblyfilters:${coverageExclude}"`
      ]
    );
  });
