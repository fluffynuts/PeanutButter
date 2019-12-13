const path = require("path"),
  gulp = requireModule("gulp-with-help"),
  lsR = requireModule("ls-r"),
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
  "update-test-utils-mvc-files",
  "Updates the files section of PeanutButter.TestUtils.MVC/Package.nuspec to include all source .cs files",
  () => {
    const project = "source/TestUtils/PeanutButter.TestUtils.MVC",
      nuspec = `${project}/Package.nuspec`;
    return gulp
      .src(nuspec)
      .pipe(throwIfNoFiles(`Nuspec not found at: ${nuspec}`))
      .pipe(
        editXml(xml => {
          const sourceFolder = `${project}`,
            projectFullPath = path.resolve(project),
            files = lsR(sourceFolder)
              .filter(p => {
                return p.match(/\.cs$/);
              })
              .map(p => p.replace(projectFullPath, ""))
              .map(p => p.replace(/^\\/, ""));

          xml.package.files = [{ file: [] }];
          const fileNode = xml.package.files[0].file;
          fileNode.push({
            $: {
              src: "icon.png",
              target: ""
            }
          })
          files.forEach(relPath => {
            fileNode.push({
              $: {
                src: relPath,
                target: `content\\${relPath}`
              }
            });
          });

          return xml;
        }, opts)
      )
      .pipe(gulp.dest(project));
  }
);
