/// <reference path="../node_modules/zarro/types.d.ts" />
import { Stream } from "stream";

(function () {
  const
    gulp = requireModule<Gulp>("gulp"),
    { build, clean, nugetPush, pack, run } = requireModule<DotNetCli>("dotnet-cli"),
    { ExecStepContext } = require("exec-step"),
    { ls, fileExists, readTextFileLines } = require("yafs"),
    runSequence = requireModule<RunSequence>("run-sequence"),
    sleep = requireModule<Sleep>("sleep"),
    del = require("del"),
    exec = requireModule<Exec>("exec"),
    system = requireModule<System>("system"),
    path = require("path"),
    { findTool } = requireModule<TestUtilFinder>("test-util-finder"),
    PQueue = require("p-queue").default,
    env = requireModule<Env>("env"),
    gutil = requireModule<GulpUtil>("gulp-util"),
    { envFlag } = requireModule<EnvHelpers>("env-helpers"),
    runInParallel = requireModule<RunInParallel>("run-in-parallel"),
    resolveNugetApiKey = requireModule<ResolveNugetApiKey>("resolve-nuget-api-key"),
    nugetReleaseDir = ".release-packages";

  gulp.task("clean-old-packages", async () => {
    const paths = await del("**/*.nupkg.bak") as string[];
    paths.forEach(p => {
      console.log(`removed: ${p}`);
    });
  });

  gulp.task("build-binaries-for-nuget-packages", [ "prebuild" ], async () => {
    const
      target = "source/PeanutButter.sln",
      configuration = "Release",
      opts = {
        target,
        configuration,
        verbosity: "minimal",
        stdout: console.log.bind(console),
        stderr: console.error.bind(console)
      } as DotNetBuildOptions;
    await clean(opts);
    await build(opts);
  });

  function fail(stream: Stream, msg: string) {
    stream.emit("error", new Error(msg));
  }

  gulp.task("build-binary-nuget-packages", async () => {
    const projects = await findPackableProjects();
    const tasks = [] as AsyncVoidVoid[];
    for (const project of projects) {
      const nuspec = await tryFindPackageNuspecFor(project);
      if (!nuspec) {
        throw new Error(`Can't find associated Package.nuspec for: '${project}'`);
      }
      tasks.push(async () => {
        const packResult = await pack({
          target: project,
          configuration: "Release",
          output: nugetReleaseDir,
          nuspec: "Package.nuspec",
          noBuild: true
        });
        if (system.isError(packResult)) {
          console.error(packResult);
          throw packResult;
        }
      });
    }
    await runInParallel(
      env.resolveNumber(env.MAX_CONCURRENCY),
      ...tasks
    );
  })
  ;


  gulp.task("build-binaries-for-nuget-packages-from-zero", [ "purge" ], function (done) {
    runSequence("build-binaries-for-nuget-packages", done);
  });

  gulp.task("test-package-build", [ "build-binaries-for-nuget-packages-from-zero" ], function (done) {
    runSequence("build-binary-nuget-packages", "test-packages-exist", done);
  });

  gulp.task("test-packages-exist", async () => {
    const nuspecs = await ls(".", {
      recurse: true,
      match: /Package.nuspec$/,
      exclude: /node_modules/
    });
    const missing = [] as string[];
    for (const nuspec of nuspecs) {
      if (nuspec.includes("_deprecated_")) {
        continue;
      }
      const version = await parseVersionFrom(nuspec);
      const container = path.dirname(nuspec);
      const projects = await ls(container, {
        match: /\.csproj$/,
        recurse: false
      });
      const project = projects[0];
      if (!project) {
        console.warn(`no project in ${container}`);
        continue;
      }
      const
        filename = path.basename(project),
        seek = filename.replace(/\.csproj$/, `.${version}.nupkg`),
        fullSeek = path.join(nugetReleaseDir, seek);

      if (!await fileExists(fullSeek)) {
        missing.push(seek);
      }
    }
    if (missing.length) {
      const s = missing.length === 1 ? "" : "s";
      const details = `- ${missing.join("\n  - ")}`;
      throw new Error(`WARNING:
  There are ${missing.length} missing package artifacts:
  ${details}`);
    }
    return Promise.resolve("wip");
  });

  async function parseVersionFrom(nuspec: string) {
    const contents = await readTextFileLines(nuspec);
    for (const line of contents) {
      const m = line.match(/\s*<version>(?<version>[0-9.]+)<\/version>/);
      if (!m) {
        continue;
      }
      return m.groups["version"];
    }
    throw new Error(`can't find version tag in ${nuspec}`);
  }

  gulp.task("clean-nuget-releasedir", function () {
    return del(nugetReleaseDir);
  });

  gulp.task(
    "build-nuget-packages",
    [ "clean-nuget-releasedir", "build-binaries-for-nuget-packages" ],
    function (done) {
      runSequence(
        "update-tempdb-runner-files",
        "increment-package-versions",
        "build-binary-nuget-packages",
        done
      );
    }
  );

  gulp.task("increment-package-versions", () => {
    const name = "NO_VERSION_INCREMENT";
    if (envFlag(name, false)) {
      gutil.log(gutil.colors.red(`Skipping package version increment: env var ${name} is set to ${process.env[name]}`));
      return Promise.resolve();
    }
    const
      incrementer = "NugetPackageVersionIncrementer";
    const util = findTool(`${incrementer}.csproj`, `source/${incrementer}`);
    if (!util) {
      throw new Error(`Can't find ${incrementer}.csproj under source/${incrementer}`);
    }
    return run({
      target: util,
      args: [ "source" ]
    });
    // return system(dotnet, [ util, "source" ]);
  });

  gulp.task("release", [ "build-nuget-packages" ], function (done) {
    runSequence("push-packages", "commit-release", "tag-and-push", done);
  });

  gulp.task("after-manual-push", done => {
    runSequence("commit-release", "tag-and-push", done);
  });

  gulp.task("push-packages", async () => {
    await pushPackages(false);
  });

  async function listReleasePackages() {
    const built = await ls(nugetReleaseDir, {
      match: /\.nupkg$/,
      fullPaths: true
    });
    return built.filter(p => !p.endsWith(".symbols.nupkg"));
  }

  async function pushPackages(skipDuplicates: boolean) {
    const tasks = [] as AsyncVoidVoid[];
    for (const pkg of await listReleasePackages()) {
      tasks.push(async () => {
        await nugetPush({
          target: pkg,
          apiKey: await resolveNugetApiKey("nuget.org"),
          skipDuplicates: false
        });
      });
    }
    await runInParallel(2, ...tasks);
  }

  gulp.task(
    "re-push-packages",
    "Attempt re-push of all packages, skipping those already found at nuget.org",
    async () => {
      await pushPackages(true);
    });

  gulp.task("_test_", async () => {
    console.log(await listLocalPackageFiles());
  });

  async function listLocalPackageFiles() {
    const nupkgs = await ls(".", {
      recurse: true,
      match: /\.nupkg$/,
      exclude: /node_modules/
    });
    return nupkgs;
  }

  async function findPackableProjects() {
    const allProjects = await ls(".", {
      match: /\.csproj$/,
      exclude: /node_modules/,
      recurse: true
    });
    const result = [] as string[];
    for (const project of allProjects) {
      if (project.includes("_deprecated_")) {
        console.warn(`ignoring ${project}`);
        continue;
      }
      const nuspec = await tryFindPackageNuspecFor(project);
      if (nuspec) {
        result.push(project);
      }
    }
    return result.sort();
  }

  const nuspecCache = {} as Dictionary<string>;

  async function tryFindPackageNuspecFor(
    project: string
  ): Promise<Optional<string>> {
    const cached = nuspecCache[project];
    if (cached) {
      return cached;
    }
    const
      container = path.dirname(project),
      seek = path.join(container, "Package.nuspec");
    if (await fileExists(seek)) {
      return nuspecCache[project] = seek;
    }
    return undefined;
  }
})();
