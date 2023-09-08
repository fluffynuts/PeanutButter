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
  "update-test-utils-mvc-files",
  "Updates the files section of PeanutButter.TestUtils.MVC/Package.nuspec to include all source .cs files",
  () => {
    const project = "source/TestUtils/PeanutButter.TestUtils.MVC",
      nuspec = `${ project }/Package.nuspec`;
    return gulp
      .src(nuspec)
      .pipe(throwIfNoFiles(`Nuspec not found at: ${ nuspec }`))
      .pipe(
        editXml(xml => {
          const sourceFolder = `${ project }`,
            projectFullPath = path.resolve(project),
            files = lsSync(sourceFolder, { recurse: true, entities: FsEntities.files })
              .filter(p => {
                return p.match(/\.cs$/);
              })
              .map(p => p.replace(projectFullPath, ""))
              .map(p => p.replace(/^\\/, ""));

          xml.package.files = [ { file: [] } ];
          const fileNode = xml.package.files[ 0 ].file;
          fileNode.push({
            $: {
              src: "icon.png",
              target: ""
            }
          });
          // ffs, nuget people can't stick with one way to do things
          // -> projects using PackageReference nodes copy out files from
          //    contentFiles. Well done, so now each file has to be included
          //    twice.
          [ "content", "contentFiles" ].forEach(targetBase => {
            files.forEach(relPath => {
              fileNode.push({
                $: {
                  src: relPath,
                  target: `${ targetBase }\\${ relPath }`
                }
              });
            });
          });

          return xml;
        }, opts)
      )
      .pipe(gulp.dest(project));
  }
);
