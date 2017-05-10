const
  gulp = requireModule("gulp-with-help"),
  downloadNuget = requireModule("nuget-downloader");
exec = requireModule("exec"),
  path = require("path"),
  fs = require("fs"),
  toolsFolder = "tools",
  nugetExe = "nuget.exe",
  del = require("del"),
  requiredTools = [
    "nunit.console",
    "opencover",
    "reportgenerator"
  ];

gulp.task("make-tools-folder-if-missing", false, () => {
  return new Promise((resolve, reject) => {
    if (!fs.existsSync(toolsFolder)) {
      fs.mkdirSync(toolsFolder);
    }
    resolve();
  });
});

gulp.task("clean-tools", 
  "Cleans out the tools folder of all tools", 
  [ "make-tools-folder-if-missing" ],
  () => {
  const dirs = fs.readdirSync(toolsFolder)
    .map(p => path.join(toolsFolder, p))
    .filter(p => {
      const stat = fs.lstatSync(p);
      return stat.isDirectory();
    });
  return del(dirs);
});

gulp.task("update-tools-nuget", "Updates nuget.exe in the tools folder", () => {
  return downloadNuget(toolsFolder);
});

gulp.task("install-tools",
  "Installs required tools to /tools folder",
  ["clean-tools", "update-tools-nuget"],
  () => {
    const
      promises = requiredTools.map(tool => exec(
        nugetExe,
        ["install", tool],
        { cwd: toolsFolder }
      ));
    return Promise.all(promises);
  });