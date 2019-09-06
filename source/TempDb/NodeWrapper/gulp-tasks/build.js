const
    gulp = requireModule("gulp-with-help"),
    ts = require("gulp-typescript");

const tsProject = ts.createProject("tsconfig.json");

gulp.task("build", "Builds the dist artifacts", ["clean"], () => {
    return gulp.src("src/**/*.ts")
        .pipe(tsProject())
        .pipe(gulp.dest("dist"));
});
