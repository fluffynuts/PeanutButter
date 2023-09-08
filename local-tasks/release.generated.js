"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
Object.defineProperty(exports, "__esModule", { value: true });
(function () {
    var _this = this;
    var gulp = requireModule("gulp"), lsSync = require("yafs").lsSync, runSequence = requireModule("run-sequence"), msbuild = requireModule("gulp-msbuild"), sleep = requireModule("sleep"), del = require("del"), exec = requireModule("exec"), spawn = requireModule("System"), path = require("path"), findTool = requireModule("testutil-finder").findTool, PQueue = require("p-queue").default, env = requireModule("env"), gutil = requireModule("gulp-util"), envFlag = requireModule("env-helpers").envFlag, usingDotnetCore = env.resolveFlag("DOTNET_CORE"), commonConfig = {
        toolsVersion: "auto",
        stdout: true,
        verbosity: "minimal",
        errorOnFail: true,
        architecture: "x64"
    };
    gulp.task("clean-old-packages", function () { return __awaiter(_this, void 0, void 0, function () {
        var paths;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4 /*yield*/, del("**/*.nupkg.bak")];
                case 1:
                    paths = _a.sent();
                    paths.forEach(function (p) {
                        console.log("removed: ".concat(p));
                    });
                    return [2 /*return*/];
            }
        });
    }); });
    gulp.task("build-binaries-for-nuget-packages", ["prebuild"], function () {
        var config = Object.assign({}, commonConfig); // FIXME: should type GulpMsBuildOptions
        config.targets = ["Clean", "Build"];
        config.configuration = "Release"; // TODO: change back to Release once all .NugetPackage.csproj projects have been purged
        config.toolsVersion = "auto";
        return gulp.src(["**/*.sln", "!**/node_modules/**/*.sln", "!./tools/**/*.sln"])
            .pipe(msbuild(config));
    });
    function fail(stream, msg) {
        stream.emit("error", new Error(msg));
    }
    function processPathsWith(getNugetArgsFor) {
        var es = require("event-stream"); // fixme: type es.through
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
            Promise.resolve(restoreTool)
                .then(function (nuget) {
                var queue = new PQueue({ concurrency: 3 });
                queue.addAll(files.map(function (pkgPath) {
                    var args = getNugetArgsFor(pkgPath);
                    if (args) {
                        return function () {
                            return retry(function () { return exec(nuget, args); }, 10, function (e) {
                                if (e && e.info) {
                                    var errors = e.info.stderr.join("\n").trim();
                                    if (errors.match(/: 409 \(/)) {
                                        console.warn(errors);
                                        return true;
                                    }
                                }
                                return false;
                            });
                        };
                    }
                    else {
                        return function () { return Promise.reject("Can't determine nuget args for ".concat(pkgPath)); };
                    }
                })).then(function () { return stream.emit("end"); }).catch(function (e) {
                    console.error(e);
                    stream.emit("error", new Error(e));
                });
            });
        });
        return stream;
    }
    function retry(func, times, considerFailureAsSuccess) {
        return __awaiter(this, void 0, void 0, function () {
            var i, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        i = 0;
                        _a.label = 1;
                    case 1:
                        if (!(i < times)) return [3 /*break*/, 9];
                        _a.label = 2;
                    case 2:
                        _a.trys.push([2, 4, , 8]);
                        return [4 /*yield*/, func()];
                    case 3:
                        _a.sent();
                        return [2 /*return*/];
                    case 4:
                        e_1 = _a.sent();
                        if (considerFailureAsSuccess && considerFailureAsSuccess(e_1)) {
                            return [2 /*return*/];
                        }
                        console.error(e_1);
                        if (!(i < (times - 1))) return [3 /*break*/, 6];
                        console.log("will retry in 1s");
                        return [4 /*yield*/, sleep(1000)];
                    case 5:
                        _a.sent();
                        return [3 /*break*/, 7];
                    case 6:
                        console.error("giving up");
                        throw e_1;
                    case 7: return [3 /*break*/, 8];
                    case 8:
                        i++;
                        return [3 /*break*/, 1];
                    case 9: return [2 /*return*/];
                }
            });
        });
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
        return processPathsWith(function (filePath) {
            var result = ["nuget", "push", filePath, "--source", "nuget.org", "--timeout", "300", "--skip-duplicate"];
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
    var pushNugetPackages = usingDotnetCore
        ? pushNugetPackagesWithDotNet
        : pushNugetPackagesWithNugetExe;
    var nugetReleaseDir = ".release-packages";
    function buildNugetPackagesWithNugetExe(includeSymbols) {
        return processPathsWith(function (filePath) {
            var args = ["pack", filePath, "-NonInteractive", "-Verbosity", "Detailed", "-OutputDirectory", nugetReleaseDir];
            if (includeSymbols) {
                args.push("-Symbols");
            }
            return args;
        });
    }
    function findProjectNextTo(nuspec) {
        var dir = path.dirname(nuspec), contents = lsSync(dir), project = contents.filter(function (o) { return o.match(/\.(cs|vb)proj$/); })[0];
        if (!project) {
            throw new Error("Can't find project in ".concat(dir));
        }
        return path.join(dir, project);
    }
    function buildNugetPackagesWithDotNet(includeSymbols) {
        return processPathsWith(function (filePath) {
            var projectPath = findProjectNextTo(filePath);
            var args = ["pack", projectPath, "-p:NuspecFile=".concat(filePath), "--verbosity", "minimal", "--output", nugetReleaseDir];
            if (includeSymbols) {
                args.push("--include-symbols");
            }
            return args;
        });
    }
    var buildNugetPackages = usingDotnetCore ? buildNugetPackagesWithDotNet : buildNugetPackagesWithNugetExe;
    gulp.task("build-source-nuget-packages", function () {
        return gulp.src(["**/PeanutButter.TestUtils.MVC/*.nuspec"])
            .pipe(buildNugetPackages(false));
    });
    gulp.task("build-binary-nuget-packages", function () {
        return gulp.src(["**/source/**/*.nuspec", "!**/packages/**/*.nuspec", "!**/_deprecated_/**",
            "!**/PeanutButter.TestUtils.MVC/**"])
            .pipe(buildNugetPackages(true));
    });
    gulp.task("build-binaries-for-nuget-packages-from-zero", ["purge"], function (done) {
        runSequence("build-binaries-for-nuget-packages", done);
    });
    gulp.task("test-package-build", ["build-binaries-for-nuget-packages-from-zero"], function (done) {
        runSequence("build-binary-nuget-packages", "build-source-nuget-packages", "test-packages-exist", done);
    });
    gulp.task("test-packages-exist", function () {
        return Promise.resolve("wip");
    });
    gulp.task("clean-nuget-releasedir", function () {
        return del(nugetReleaseDir);
    });
    gulp.task("build-nuget-packages", ["clean-nuget-releasedir", "build-binaries-for-nuget-packages"], function (done) {
        runSequence("update-tempdb-runner-files", "update-test-utils-mvc-files", "increment-package-versions", "build-binary-nuget-packages", "build-source-nuget-packages", done);
    });
    gulp.task("increment-package-versions", function () {
        var name = "NO_VERSION_INCREMENT";
        if (envFlag(name, false)) {
            gutil.log(gutil.colors.red("Skipping package version increment: env var ".concat(name, " is set to ").concat(process.env[name])));
            return Promise.resolve();
        }
        var incrementer = "NugetPackageVersionIncrementer";
        var util = findTool("".concat(incrementer, ".exe"), "source/".concat(incrementer));
        return spawn(util, ["source"]);
    });
    gulp.task("release", ["build-nuget-packages"], function (done) {
        runSequence("push-packages", "commit-release", "tag-and-push", done);
    });
    gulp.task("after-manual-push", function (done) {
        runSequence("commit-release", "tag-and-push", done);
    });
    gulp.task("push-packages", function () {
        return gulp.src([nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg"])
            .pipe(pushNugetPackages(false));
    });
    gulp.task("re-push-packages", "Attempt re-push of all packages, skipping those already found at nuget.org", function () {
        return gulp.src([nugetReleaseDir + "/*.nupkg", "!" + nugetReleaseDir + "/*.symbols.nupkg", "!**/packages/**/*.nupkg"])
            .pipe(pushNugetPackages(true));
    });
})();
