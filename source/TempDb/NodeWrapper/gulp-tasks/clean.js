const
    gulp = requireModule("gulp-with-help"),
    rimraf = require("rimraf");

gulp.task("clean", "Removes dist artifacts", cb => {
    rimraf("dist", cb);
});
