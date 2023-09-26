var gulp = requireModule("gulp");

gulp.task("echo", async () => {
  console.log(`FOO is: ${process.env.FOO}`);
});
