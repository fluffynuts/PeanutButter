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
        addTag(`v${version}`, `:bookmark: release v${version}`)
          .then(pushTags)
          .then(push)
          .then(resolve)
        .catch(err => reject(err));
        return xml;
      })
    );
  });
});

function addTag(tag, msg) {
  return new Promise((resolve, reject) => {
    git.addAnnotatedTag(tag, msg, err => {
        return err ? reject(`Unable to create tag: ${err}`) : resolve();
    });
  });
}

function push() {
  return new Promise((resolve, reject) => {
    git.push("origin", err => {
        return err ? reject(`Unable to push: ${err}`) : resolve;
    });
  });
}

function pushTags() {
  return new Promise((resolve, reject) => {
    git.pushTags("origin", err => {
        return err ? reject(`Unable to push tags: ${err}`) : resolve();
    });
  });
}

