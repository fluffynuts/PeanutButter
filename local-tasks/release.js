var
  fs = require("fs"),
  gulp = require("gulp"),
  runSequence = requireModule("run-sequence"),
  msbuild = require("gulp-msbuild"),
  sleep = requireModule("sleep"),
  del = require("del"),
  exec = requireModule("exec"),
  spawn = requireModule("spawn"),
  es = require("event-stream"),
  lsr = requireModule("ls-r"),
  path = require("path"),
  findTool = requireModule("testutil-finder").findTool,
  PQueue = require("p-queue").default,
  env = requireModule("env"),
  gutil = requireModule("gulp-util"),
  { envFlag } = requireModule("env-helpers"),
  usingDotnetCore = env.resolveFlag("DOTNET_CORE"),
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
    var restoreTool = requireModule("resolve-nuget")();
    Promise.resolve(restoreTool).then(function (nuget) {
      var queue = new PQueue({ concurrency: 3 });
      queue.addAll(files.map(pkgPath => {
        var args = getNugetArgsFor(pkgPath);
        if (args) {
          return () => {
            return retry(() => exec(nuget, args), 10, e => {
              if (e && e.info) {
                const errors = e.info.stderr.join("\n").trim();
                if (errors.match(/: 409 \(/)) {
                  console.warn(errors);
                  return true;
                }
              }
              return false;
            });
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

async function retry(func, times, considerFailureAsSuccess) {
  for (let i = 0; i < times; i++) {
    try {
      await func();
      return;
    } catch (e) {
      if (considerFailureAsSuccess &&
        considerFailureAsSuccess(e)) {
        return;
      }
      console.error(e);
      if (i < (times - 1)) {
        console.log("will retry in 1s");
        await sleep(1000);
      } else {
        console.error("giving up");
        throw e;
      }
    }
  }
}

function pushNugetPackagesWithNugetExe(skipDuplicates) {
  return processPathsWith(function (filePath) {
    var result = ["push", filePath, "-NonInteractive", "-Source", "nuget.org", "-Timeout", "900", "-SkipDuplicate"];
    if (skipDuplicates) {
      result.push("-SkipDuplicate");
    }
    if (process.env.NUGET_API_KEY) {
        result.push("-ApiKey");
        result.push(process.env.NUGET_API_KEY);
    }
    return result;
  });
}

function pushNugetPackagesWithDotNet(skipDuplicates) {
  return processPathsWith(filePath => {
    var result = [ "nuget", "push", filePath, "--source", "nuget.org", "--timeout", "300", "--skip-duplicate" ];
    if (skipDuplicates) {
      result.push("--skip-duplicates");
    }
    if (process.env.NUGET_API_KEY) {
      result.push("--api-key");
      result.push(process.env.NUGET_API_KEY);
    }
    return result;
  });
}

const pushNugetPackages = usingDotnetCore
  ? pushNugetPackagesWithDotNet
  : pushNugetPackagesWithNugetExe

var nugetReleaseDir = ".release-packages";
function buildNugetPackagesWithNugetExe(includeSymbols) {
  return processPathsWith(function (filePath) {
    var args = ["pack", filePath, "-NonInteractive", "-Verbosity", "Detailed", "-OutputDirectory", nugetReleaseDir];
    if (includeSymbols) {
      args.push("-Symbols");
    }
    return args;
  })
}

function findProjectNextTo(nuspec) {
  const
    dir = path.dirname(nuspec),
    contents = fs.readdirSync(dir),
    project = contents.filter(o => o.match(/\.(cs|vb)proj$/))[0];
  if (!project) {
    throw new Error(`Can't find project in ${dir}`);
  }
  return path.join(dir, project);
}

function buildNugetPackagesWithDotNet(includeSymbols) {
  return processPathsWith(filePath => {
    const projectPath = findProjectNextTo(filePath);
    var args = ["pack", projectPath, `-p:NuspecFile=${filePath}`, "--verbosity", "minimal", "--output", nugetReleaseDir ];
    if (includeSymbols) {
      args.push("--include-symbols");
    }
    return args;
  });
}

const buildNugetPackages = usingDotnetCore
  ? buildNugetPackagesWithDotNet
  : buildNugetPackagesWithNugetExe;

gulp.task("build-source-nuget-packages", function () {
  return gulp.src(["**/PeanutButter.TestUtils.MVC/*.nuspec"])
    .pipe(buildNugetPackages(false));
});

gulp.task("build-binary-nuget-packages", function () {
  return gulp.src(["**/source/**/*.nuspec",
    "!**/packages/**/*.nuspec",
    "!**/_deprecated_/**",
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
  const name = "NO_VERSION_INCREMENT";
  if (envFlag(name, false)) {
    gutil.log(gutil.colors.red(`Skipping package version increment: env var ${name} is set to ${process.env[name]}`));
    return Promise.resolve();
  }
  var util = findTool("NugetPackageVersionIncrementer.exe", "source");
  return spawn(util, ["source"]);
});

gulp.task("release", ["build-nuget-packages"], function (done) {
  runSequence("push-packages", "commit-release", "tag-and-push", done);
});

gulp.task("after-manual-push", done => {
  runSequence("commit-release", "tag-and-push", done);
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

