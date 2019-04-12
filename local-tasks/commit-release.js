const gulp = requireModule("gulp-with-help"),
  gutil = require("gulp-util"),
  editXml = require("gulp-edit-xml"),
  Git = require("simple-git"),
  git = new Git(),
  containingFolder = "source/Utils/PeanutButter.Utils"; // track PB.Utils for tag version

gulp.task("commit-release", () => {
  return new Promise((resolve, reject) => {
    gulp.src(`${containingFolder}/Package.nuspec`).pipe(
      editXml(xml => {
        const node = xml.package.metadata[0].version,
          version = node[0].trim();

        gutil.log(gutil.colors.cyan(`Committing release ${version}`));
        git.add("./*", err => {
          if (err) {
            reject(`Unable to add all files: ${err}`);
          }
          git.commit(`:bookmark: release version ${version}`, err => {
            return err
              ? reject(`Unable to commit release ${version}: ${err}`)
              : resolve(`Release ${version} committed`);
          });
        });
        return xml;
      })
    );
  });
});
