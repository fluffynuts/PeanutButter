const gulp = requireModule("gulp-with-help"),
  gutil = require("gulp-util"),
  editXml = require("gulp-edit-xml"),
  Git = require("simple-git"),
  git = new Git(),
  containingFolder = "source/Utils/PeanutButter.Utils"; // track PB.Utils for tag version

gulp.task("tag-and-push", () => {
  return new Promise((resolve, reject) => {
    gulp.src(`${containingFolder}/Package.nuspec`).pipe(
      editXml(xml => {
        const node = xml.package.metadata[0].version,
          version = node[0].trim();

        gutil.log(gutil.colors.cyan(`Tagging at: "v${version}"`));
        git.addAnnotatedTag(
          `v${version}`,
          `chore(release): ${version}`,
          err => {
            if (err) {
              return reject(`Unable to create tag: ${err}`);
            }
            git.pushTags("origin", err => {
              if (err) {
                return reject(`Unable to push tag: ${err}`);
              }
              return resolve();
            });
          }
        );
        return xml;
      })
    );
  });
});
