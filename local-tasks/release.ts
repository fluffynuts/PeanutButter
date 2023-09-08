/// <reference path="../node_modules/zarro/types.d.ts" />
import { Stream } from "stream";

(function () {
    const
        gulp = requireModule<Gulp>("gulp"),
        { lsSync } = require("yafs"),
        runSequence = requireModule<RunSequence>("run-sequence"),
        msbuild = requireModule<GulpMsBuild>("gulp-msbuild"),
        sleep = requireModule<Sleep>("sleep"),
        del = require("del"),
        exec = requireModule<Exec>("exec"),
        spawn = requireModule<System>("System"),
        path = require("path"),
        findTool = requireModule<TestUtilFinder>("testutil-finder").findTool,
        PQueue = require("p-queue").default,
        env = requireModule<Env>("env"),
        gutil = requireModule<GulpUtil>("gulp-util"),
        { envFlag } = requireModule<EnvHelpers>("env-helpers"),
        usingDotnetCore = env.resolveFlag("DOTNET_CORE"),
        commonConfig = {
            toolsVersion: "auto",
            stdout: true,
            verbosity: "minimal",
            errorOnFail: true,
            architecture: "x64"
        };


    gulp.task("clean-old-packages", async () => {
        const paths = await del("**/*.npkg.bak") as string[];
        paths.forEach(p => {
            console.log(`removed: ${ p }`);
        });
    });

    gulp.task("build-binaries-for-nuget-packages", [ "prebuild" ], () => {
        const config = Object.assign({}, commonConfig) as any; // FIXME: should type GulpMsBuildOptions
        config.targets = [ "Clean", "Build" ];
        config.configuration = "Release"; // TODO: change back to Release once all .NugetPackage.csproj projects have been purged
        config.toolsVersion = "auto";
        return gulp.src([ "**/*.sln", "!**/node_modules/**/*.sln", "!./tools/**/*.sln" ])
            .pipe(msbuild(config));
    });

    function fail(stream: Stream, msg: string) {
        stream.emit("error", new Error(msg));
    }

    function processPathsWith(getNugetArgsFor: (s: string) => string[]) {
        const
            es = require("event-stream") as any; // fixme: type es.through
        const files = [];
        const stream = es.through(function (this: Stream, file: any) {
            if (!file) {
                fail(stream, "file may not be empty or undefined");
            }
            const filePath = file.history[0];
            files.push(filePath);
            this.emit("data", file);
        }, function () {
            const restoreTool = requireModule<ResolveNuget>("resolve-nuget")();
            Promise.resolve(restoreTool)
                .then(function (nuget) {
                    const queue = new PQueue({ concurrency: 3 });
                    queue.addAll(files.map(pkgPath => {
                            const args = getNugetArgsFor(pkgPath);
                            if (args) {
                                return () => {
                                    return retry(() => exec(nuget, args), 10, (e: any) => {
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
                                return () => Promise.reject(`Can't determine nuget args for ${ pkgPath }`);
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

    function pushNugetPackagesWithNugetExe(skipDuplicates: boolean) {
        return processPathsWith((filePath: string): string[] => {
            const result = [ "push", filePath, "-NonInteractive", "-Source", "nuget.org", "-Timeout", "900", "-SkipDuplicate" ];
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

    function pushNugetPackagesWithDotNet(
        skipDuplicates: boolean
    ) {
        return processPathsWith(filePath => {
            const result = [ "nuget", "push", filePath, "--source", "nuget.org", "--timeout", "300", "--skip-duplicate" ];
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

    const nugetReleaseDir = ".release-packages";

    function buildNugetPackagesWithNugetExe(includeSymbols: boolean) {
        return processPathsWith(function (filePath) {
            const args = [ "pack", filePath, "-NonInteractive", "-Verbosity", "Detailed", "-OutputDirectory", nugetReleaseDir ];
            if (includeSymbols) {
                args.push("-Symbols");
            }
            return args;
        })
    }

    function findProjectNextTo(nuspec: string) {
        const
            dir = path.dirname(nuspec),
            contents = lsSync(dir) as string[],
            project = contents.filter(o => o.match(/\.(cs|vb)proj$/))[0];
        if (!project) {
            throw new Error(`Can't find project in ${ dir }`);
        }
        return path.join(dir, project);
    }

    function buildNugetPackagesWithDotNet(includeSymbols: boolean) {
        return processPathsWith(filePath => {
            const projectPath = findProjectNextTo(filePath);
            const args = [ "pack", projectPath, `-p:NuspecFile=${ filePath }`, "--verbosity", "minimal", "--output", nugetReleaseDir ];
            if (includeSymbols) {
                args.push("--include-symbols");
            }
            return args;
        });
    }

    const buildNugetPackages = usingDotnetCore ? buildNugetPackagesWithDotNet : buildNugetPackagesWithNugetExe;

    gulp.task("build-source-nuget-packages", function () {
        return gulp.src([ "**/PeanutButter.TestUtils.MVC/*.nuspec" ])
            .pipe(buildNugetPackages(false));
    });

    gulp.task("build-binary-nuget-packages", function () {
        return gulp.src([ "**/source/**/*.nuspec", "!**/packages/**/*.nuspec", "!**/_deprecated_/**", /* source */
            "!**/PeanutButter.TestUtils.MVC/**" ])
            .pipe(buildNugetPackages(true));
    });

    gulp.task("build-binaries-for-nuget-packages-from-zero", [ "purge" ], function (done) {
        runSequence("build-binaries-for-nuget-packages", done);
    });

    gulp.task("test-package-build", [ "build-binaries-for-nuget-packages-from-zero" ], function (done) {
        runSequence("build-binary-nuget-packages", "build-source-nuget-packages", "test-packages-exist", done);
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
                "update-test-utils-mvc-files",
                "increment-package-versions",
                "build-binary-nuget-packages",
                "build-source-nuget-packages",
                done
            );
        }
    );

    gulp.task("increment-package-versions", () => {
        const name = "NO_VERSION_INCREMENT";
        if (envFlag(name, false)) {
            gutil.log(gutil.colors.red(`Skipping package version increment: env var ${ name } is set to ${ process.env[name] }`));
            return Promise.resolve();
        }
        const util = findTool("NugetPackageVersionIncrementer.exe", "source");
        return spawn(util, [ "source" ]);
    });

    gulp.task("release", [ "build-nuget-packages" ], function (done) {
        runSequence("push-packages", "commit-release", "tag-and-push", done);
    });

    gulp.task("after-manual-push", done => {
        runSequence("commit-release", "tag-and-push", done);
    });

    gulp.task("push-packages", () => {
        return gulp.src([ nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg" ])
            .pipe(pushNugetPackages(false));
    });

    gulp.task("re-push-packages", "Attempt re-push of all packages, skipping those already found at nuget.org", () => {
        return gulp.src([ nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg" ])
            .pipe(pushNugetPackages(true));
    });
})();
