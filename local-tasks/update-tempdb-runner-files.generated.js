/// <reference path="../node_modules/zarro/types.d.ts" />
(function () {
    var path = require("path"), gulp = requireModule("gulp"), _a = require("yafs"), lsSync = _a.lsSync, FsEntities = _a.FsEntities, throwIfNoFiles = requireModule("throw-if-no-files"), opts = {
        parserOptions: {},
        builderOptions: {
            headless: true,
            renderOpts: {
                pretty: true
            }
        }
    }, editXml = require("gulp-edit-xml");
    gulp.task("update-tempdb-runner-files", "Updates the files section of PeanutButter.TempDb.Runner/Package.nuspec to include all Release binaries", function () {
        var project = "source/TempDb/PeanutButter.TempDb.Runner", nuspec = "".concat(project, "/Package.nuspec");
        return gulp
            .src(nuspec)
            .pipe(throwIfNoFiles("Nuspec not found at: ".concat(nuspec)))
            .pipe(editXml(function (xml) {
            var releaseFolder = "".concat(project, "/bin/Release"), projectFullPath = path.resolve(project), files = lsSync(releaseFolder, {
                recurse: true,
                entities: FsEntities.files,
                fullPaths: true
            })
                .map(function (p) { return p.replace(projectFullPath, ""); })
                .map(function (p) { return p.replace(/^\\/, ""); });
            if (files.filter(function (p) { return p.match(/\.dll$/); }).length === 0) {
                throw new Error("No assemblies found under ".concat(releaseFolder));
            }
            xml.package.files = [{ file: [] }];
            var fileNode = xml.package.files[0].file;
            fileNode.push({
                $: {
                    src: "icon.png",
                    target: ""
                }
            });
            files
                .filter(filterTempDbRunnerFile)
                .forEach(function (relPath) {
                fileNode.push({
                    $: {
                        src: relPath,
                        target: targetFor(relPath)
                    }
                });
            });
            return xml;
        }, opts))
            .pipe(gulp.dest(project));
    });
    function filterTempDbRunnerFile(filepath) {
        if ((filepath || "").match(/netcoreapp.*\\PeanutButter.TempDb.Runner.exe$/)) {
            console.log("-- ignoring: ".concat(filepath));
            return false;
        }
        return true;
    }
    function targetFor(relPath) {
        var next = false;
        var parts = relPath.split(path.sep), targetFramework = parts.reduce(function (acc, cur) {
            if (cur === "Release") {
                next = true;
                return acc;
            }
            if (next) {
                next = false;
                return cur;
            }
            else {
                return acc;
            }
        }, null);
        if (!targetFramework) {
            console.log({
                parts: parts
            });
            throw new Error("Can't determine target framework for path: ".concat(relPath, " (should be after 'Release')"));
        }
        var pathParts = relPath.split(/[\/|\\]/), start = pathParts.indexOf("Release") + 2, subPath = pathParts.slice(start).join("/"), subFolder = path.dirname(subPath);
        return subFolder === "."
            ? "lib/".concat(targetFramework)
            : "lib/".concat(targetFramework, "/").concat(subFolder);
    }
})();
