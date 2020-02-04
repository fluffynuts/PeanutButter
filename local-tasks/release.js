var
  fs = require("fs")
gulp = require("gulp"),
  gdebug = require("gulp-debug"),
  runSequence = requireModule("run-sequence"),
  msbuild = require("gulp-msbuild")
sleep = requireModule("sleep"),
  del = require("del"),
  exec = requireModule("exec"),
  spawn = requireModule("spawn"),
  findLocalNuget = requireModule("find-local-nuget"),
  es = require("event-stream"),
  lsr = requireModule("ls-r"),
  path = require("path"),
  findTool = requireModule("testutil-finder").findTool,
  PQueue = require("p-queue").default,
  commonConfig = {
    toolsVersion: "auto",
    stdout: true,
    verbosity: "minimal",
    errorOnFail: true,
    architecture: "x64"
  };


gulp.task("clean-old-packages", function () {
  return del("**/*.nupkg.bak").then(function (paths) {
    paths.forEach(function (p) {
      console.log("removed: " + p);
    });
  });
});

gulp.task("build-binaries-for-nuget-packages", ["prebuild"], function () {
  var config = Object.assign({}, commonConfig);
  config.targets = ["Clean", "Build"];
  config.configuration = "Release"; // TODO: change back to Release once all .NugetPackage.csproj projects have been purged
  config.toolsVersion = "auto";
  return gulp.src(["**/*.sln", "!**/node_modules/**/*.sln", "!./tools/**/*.sln"])
    .pipe(msbuild(config));
});

function processPathsWith(getNugetArgsFor) {
  var files = [];
  var stream = es.through(function (file) {
    if (!file) {
      fail(stream, "file may not be empty or undefined");
    }
    var filePath = file.history[0];
    files.push(filePath);
    this.emit("data", file);
  }, function () {
    findLocalNuget().then(function (nuget) {
      var queue = new PQueue({ concurrency: 3 });
      queue.addAll(files.map(pkgPath => {
        var args = getNugetArgsFor(pkgPath);
        if (args) {
          return () => {
            return retry(() => exec(nuget, args), 10);
          };
        } else {
          return () => Promise.reject(`Can't determine nuget args for ${pkgPath}`);
        }
      })).then(() => stream.emit("end"))
        .catch(e => {
          console.error(e);
          stream.emit("error", new Error(e));
        });
    });
  });
  return stream;
}

async function retry(func, times) {
  for (let i = 0; i < times; i++) {
    try {
      await func();
      return;
    } catch (e) {
      console.error(e);
      if (i < (times - 1)) {
        console.log("will retry in 1s");
        await sleep(1000);
      } else {
        console.error("giving up");
      }
    }
  }
}

function pushNugetPackages(skipDuplicates) {
  return processPathsWith(function (filePath) {
    var result = ["push", filePath, "-NonInteractive", "-Source", "https://www.nuget.org", "-Timeout", "30"];
    if (skipDuplicates) {
      result.push("-SkipDuplicate");
    }
    return result;
  });
}

var nugetReleaseDir = ".release-packages";
function buildNugetPackages(includeSymbols) {
  return processPathsWith(function (filePath) {
    var args = ["pack", filePath, "-NonInteractive", "-Verbosity", "Detailed", "-OutputDirectory", nugetReleaseDir];
    if (includeSymbols) {
      args.push("-Symbols");
    }
    return args;
  })
}

gulp.task("build-source-nuget-packages", function () {
  return gulp.src(["**/PeanutButter.TestUtils.MVC/*.nuspec"])
    .pipe(buildNugetPackages(false));
});

gulp.task("build-binary-nuget-packages", function () {
  return gulp.src(["**/source/**/*.nuspec",
    "!**/packages/**/*.nuspec",
    /* source */
    "!**/PeanutButter.TestUtils.MVC/**"
  ])
    .pipe(buildNugetPackages(true));
});

gulp.task("build-binaries-for-nuget-packages-from-zero", ["purge"], function (done) {
  runSequence("build-binaries-for-nuget-packages", done);
});

gulp.task("test-package-build", ["build-binaries-for-nuget-packages-from-zero"], function (done) {
  runSequence(
    "build-binary-nuget-packages",
    "build-source-nuget-packages",
    "test-packages-exist", done);
});

gulp.task("test-packages-exist", () => {
  return Promise.resolve("wip");
});

gulp.task("release-test-package", function () {
  return gulp.src([nugetReleaseDir + `/${testProject}*`])
    .pipe(pushNugetPackages());
});

gulp.task("clean-nuget-releasedir", function () {
  return del(nugetReleaseDir);
});

gulp.task("build-nuget-packages", [
  "clean-nuget-releasedir",
  "build-binaries-for-nuget-packages"
], function (done) {
  runSequence(
    "update-tempdb-runner-files",
    "update-test-utils-mvc-files",
    "increment-package-versions",
    "build-binary-nuget-packages",
    "build-source-nuget-packages",
    done);
});

gulp.task("increment-package-versions", () => {
  var util = findTool("NugetPackageVersionIncrementer.exe", "source");
  return spawn(util, ["source"]);
});

gulp.task("release", ["build-nuget-packages"], function (done) {
  runSequence("push-packages", "commit-release", "tag-and-push", done);
});

gulp.task("push-packages", () => {
  return gulp.src([nugetReleaseDir + "/*.nupkg",
  "!" + nugetReleaseDir + "/*.symbols.nupkg",
    "!**/packages/**/*.nupkg"])
    .pipe(pushNugetPackages());
});

gulp.task("re-push-packages", "Attempt re-push of all packages, skipping those already found at nuget.org", () => {
  return gulp.src([nugetReleaseDir + "/*.nupkg",
  "!" + nugetReleaseDir + "/*.symbols.nupkg",
    "!**/packages/**/*.nupkg"])
    .pipe(pushNugetPackages(true));
});

gulp.task("update-project-nugets", () => {
  var nugets = lsr("source")
    .filter(p => path.basename(p).toLowerCase() === "nuget.exe");
  return findLocalNuget().then(localNuget => {
    var bytes = fs.readFileSync(localNuget);
    nugets.forEach(n => {
      fs.writeFileSync(n, bytes);
    });
  });
});

