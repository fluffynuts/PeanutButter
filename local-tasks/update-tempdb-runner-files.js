const path = require("path"),
  gulp = requireModule("gulp-with-help"),
  { lsSync, FsEntities } = require("yafs"),
  throwIfNoFiles = requireModule("throw-if-no-files"),
  opts = {
    parserOptions: {},
    builderOptions: {
      headless: true,
      renderOpts: {
        pretty: true
      }
    }
  },
  editXml = require("gulp-edit-xml");

gulp.task(
  "update-tempdb-runner-files",
  "Updates the files section of PeanutButter.TempDb.Runner/Package.nuspec to include all Release binaries",
  () => {
    const project = "source/TempDb/PeanutButter.TempDb.Runner",
      nuspec = `${ project }/Package.nuspec`;
    return gulp
      .src(nuspec)
      .pipe(throwIfNoFiles(`Nuspec not found at: ${ nuspec }`))
      .pipe(
        editXml(xml => {
          const releaseFolder = `${ project }/bin/Release`,
            projectFullPath = path.resolve(project),
            files = lsSync(releaseFolder, { recurse: true, entities: FsEntities.files })
              .map(p => p.replace(projectFullPath, ""))
              .map(p => p.replace(/^\\/, ""));
          if (files.filter(p => p.match(/\.dll$/)).length === 0) {
            throw new Error(`No assemblies found under ${ releaseFolder }`);
          }

          xml.package.files = [ { file: [] } ];
          const fileNode = xml.package.files[ 0 ].file;
          fileNode.push({
            $: {
              src: "icon.png",
              target: ""
            }
          })
          files
            .filter(filterTempDbRunnerFile)
            .forEach(relPath => {
              fileNode.push({
                $: {
                  src: relPath,
                  target: targetFor(relPath)
                }
              });
            });

          return xml;
        }, opts)
      )
      .pipe(gulp.dest(project));
  }
);

function filterTempDbRunnerFile(filepath) {
  if (( filepath || "" ).match(/netcoreapp.*\\PeanutButter.TempDb.Runner.exe$/)) {
    console.log(`-- ignoring: ${ filepath }`);
    return false;
  }
  return true;
}

function targetFor(relPath) {
  let next = false;
  const parts = relPath.split(path.sep),
    targetFramework = parts.reduce((acc, cur) => {
      if (cur === "Release") {
        next = true;
        return acc;
      }
      if (next) {
        next = false;
        return cur;
      } else {
        return acc;
      }
    }, null);
  if (!targetFramework) {
    console.log({
      parts
    });
    throw new Error(
      `Can't determine target framework for path: ${ relPath } (should be after 'Release')`
    );
  }
  const
    pathParts = relPath.split(/[\/|\\]/),
    start = pathParts.indexOf("Release") + 2,
    subPath = pathParts.slice(start).join("/"),
    subFolder = path.dirname(subPath);
  return subFolder === "."
    ? `lib/${ targetFramework }`
    : `lib/${ targetFramework }/${ subFolder }`;
}
