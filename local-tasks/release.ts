/// <reference path="../node_modules/zarro/types.d.ts" />
import { Stream } from "stream";

(function () {
  const
    gulp = requireModule<Gulp>("gulp"),
    { build, clean, nugetPush, pack, publish, run } = requireModule<DotNetCli>("dotnet-cli"),
    { lsSync, ls, fileExists } = require("yafs"),
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
    os = require("os"),
    isWindows = os.platform() === "win32",
    usingDotnetCore = env.resolveFlag("DOTNET_CORE");

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

  function processPathsWith(
    labelPrefix: string,
    completeIcon: string,
    getNugetArgsFor: (s: string) => string[]
  ) {
    const
      { ExecStepContext } = require("exec-step"),
      config = {
        prefixes: {
          ok: completeIcon,
          fail: "💥",
          wait: "⏱️"
        }
      },
      ctx = new ExecStepContext(config),
      es = require("event-stream") as any; // fixme: type es.through
    const files = [] as string[];
    const stream = es.through(function (this: Stream, file: any) {
      if (!file) {
        fail(stream, "file may not be empty or undefined");
      }
      const filePath = file.history[0] as string;
      files.push(filePath);
      this.emit("data", file);
    }, function () {
      const restoreTool = requireModule<ResolveNuget>("resolve-nuget")();
      Promise.resolve(restoreTool)
        .then(function (nuget) {
          const queue = new PQueue({ concurrency: 3 });
          queue.addAll(files.map(pkgPath => {
              const args = getNugetArgsFor(pkgPath);
              const packageName = path.basename(pkgPath).toLowerCase() === "package.nuspec"
                ? path.basename(path.dirname(pkgPath))
                : path.basename(pkgPath);
              if (args) {
                return () => {
                  return retry(
                    () => ctx.exec(
                      `${labelPrefix}: ${packageName}`,
                      () => exec(nuget, args)
                    ),
                    10,
                    (e: any) => {
                      if (e && e.info) {
                        const errors = e.info.stderr.join("\n").trim();
                        if (errors.match(/: 409 \(/)) {
                          console.warn(errors);
                          return true;
                        }
                      }
                      return false;
                    }
                  );
                };
              } else {
                return () => Promise.reject(`Can't determine nuget args for ${pkgPath}`);
              }
            })
          ).then(
            () => stream.emit("end")
          ).catch((e: string) => {
            console.error(e);
            stream.emit("error", new Error(e));
          });
        });
    });
    return stream;
  }

  async function retry(
    func: Function,
    times: number,
    considerFailureAsSuccess: (e: any) => boolean
  ) {
    for (let i = 0; i < times; i++) {
      try {
        await func();
        return;
      } catch (e) {
        if (considerFailureAsSuccess && considerFailureAsSuccess(e)) {
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

  // function pushNugetPackagesWithNugetExe(skipDuplicates: boolean) {
  //   return processPathsWith(
  //     "pushing",
  //     "🚀",
  //     (filePath: string): string[] => {
  //       const result = [ "push", filePath, "-NonInteractive", "-Source", "nuget.org", "-Timeout", "900" ];
  //       if (skipDuplicates) {
  //         result.push("-SkipDuplicate");
  //       }
  //       result.push("-ApiKey");
  //       result.push(findNugetApiKey());
  //       return result;
  //     }
  //   );
  // }

  let apiKey = "";

  function findNugetApiKey(): string {
    const result = findNugetApiKeyForHost("nuget.org") || findGlobalNugetApiKey();
    if (!result) {
      throw new Error(`Unable to determine the nuget api key to use for upload`);
    }
    return result;
  }

  function findGlobalNugetApiKey() {
    return process.env["NUGET_API_KEY"];
  }

  function findNugetApiKeyForHost(host) {
    try {
      const json = process.env["NUGET_API_KEYS"];
      if (!json) {
        return undefined;
      }
      const map = JSON.parse(json);
      return map[host];
    } catch (e) {
      return undefined;
    }
  }

  function pushNugetPackagesWithDotNet(
    skipDuplicates: boolean
  ) {
    return processPathsWith(
      "pushing",
      "🚀",
      filePath => {
        const result = [ "nuget", "push", filePath, "--source", "nuget.org", "--timeout", "300", "--skip-duplicate" ];
        if (skipDuplicates) {
          result.push("--skip-duplicates");
        }
        result.push("--api-key");
        result.push(findNugetApiKey());
        return result;
      }
    );
  }

  const nugetReleaseDir = ".release-packages";

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
          throw packResult;
        }
      });
    }
    await runInParallel(
      env.resolveNumber(env.MAX_CONCURRENCY),
      ...tasks
    );
  });


  gulp.task("build-binaries-for-nuget-packages-from-zero", [ "purge" ], function (done) {
    runSequence("build-binaries-for-nuget-packages", done);
  });

  gulp.task("test-package-build", [ "build-binaries-for-nuget-packages-from-zero" ], function (done) {
    runSequence("build-binary-nuget-packages", "test-packages-exist", done);
  });

  gulp.task("test-packages-exist", () => {
    return Promise.resolve("wip");
  });

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

  gulp.task("push-packages", () => {
    return gulp.src([ nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg" ])
      .pipe(pushNugetPackagesWithDotNet(false));
  });

  gulp.task("re-push-packages", "Attempt re-push of all packages, skipping those already found at nuget.org", () => {
    return gulp.src([ nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg" ])
      .pipe(pushNugetPackagesWithDotNet(true));
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
        continue;
      }
      const nuspec = await tryFindPackageNuspecFor(project);
      if (nuspec) {
        result.push(project);
      }
    }
    return result;
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
