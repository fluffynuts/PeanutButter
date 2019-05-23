/*
  Welcome new user!

  To get started, copy this gulpfile.js to the root of your repo and run:
  `node gulpfile.js`
  You should be guided through the basic setup. More information in README.md. In
  particular, I highly recommend reading about how to use `local-tasks` to extend
  and / or override the default task-set.
 */
var 
  os = require("os"),
  fs = require("fs"),
  gulpTasksFolder = "gulp-tasks", // if you cloned elsewhere, you"ll need to modify this
  requireModule = global.requireModule = function(mod) {
    var modulePath = [".", gulpTasksFolder, "modules", mod].join("/");
    if (fs.existsSync(modulePath + ".js")) {
        return require(modulePath);
    } else {
        return require(mod);
    }
  };


if (!fs.existsSync(gulpTasksFolder)) {
  console.error("Either clone `gulp-tasks` to the `gulp-tasks` folder or modify this script to avoid sadness");
  process.exit(2);
}

if (!fs.existsSync("package.json")) {
  console.log(
    "You need to set up a package.json first. I'll run `npm init` for you (:"
  );
  initializeNpm();
} else if (mustInstallDeps()) {
  console.log(
    "Now we just need to install the dependencies required for gulp-tasks to run (:"
  );
  installGulpTaskDependencies().then(() =>
    console.log("You're good to go with `gulp-tasks`. Try running `npm run gulp build`")
  );
} else {
  bootstrapGulp();
}

function requiredDeps() {
    var starter = require([".", gulpTasksFolder, "start", "package.json"].join("/"));
    return Object.keys(starter.devDependencies);
}

function mustInstallDeps() {
  var
    package = require("./package.json"),
    devDeps = package.devDependencies || {},
    haveDeps = Object.keys(devDeps),
    needDeps = requiredDeps();
  return needDeps.reduce((acc, cur) => {
    return acc || haveDeps.indexOf(cur) == -1;
  }, false);
}

function initializeNpm() {
  var spawn = requireModule("spawn");
  runNpmWith(["init"])
  .then(() => spawn("cmd", [ "/c", "node", process.argv[1]]));
}

function installGulpTaskDependencies() {
  var
    findFirstMissing = function() {
      var args = Array.from(arguments);
      return args.reduce((acc, cur) => acc || (fs.existsSync(cur) ? acc : cur), undefined);
    },
    deps = requiredDeps(),
    package = require("./package.json"),
    buildTools = findFirstMissing("tools", "build-tools", ".tools", ".build-tools"),
    prepend = `cross-env BUILD_TOOLS_FOLDER=${buildTools}`;

  package.scripts["gulp"] = `${prepend} gulp`;
  package.scripts["test"] = `run-s "gulp test-dotnet"`;
  package.scripts["build"] = `run-s "gulp build"`;
  package.scripts["pack"] = `BUILD_CONFIGURATION=Release run-s "gulp pack"`;

  fs.writeFileSync("package.json", JSON.stringify(package, null, 4), { encoding: "utf8" });

  return runNpmWith(["install", "--save-dev"].concat(deps));
}

function bootstrapGulp() {
  var importNpmTasks = requireModule("import-npm-tasks");
  try {
    importNpmTasks();
    var requireDir = require("require-dir");
    requireDir("gulp-tasks");
    ["override-tasks", "local-tasks"].forEach(function(dirname) {
      if (fs.existsSync(dirname)) {
        requireDir(dirname);
      }
    });
  } catch (e) {
    if (looksLikeMissingGulpTasksModule(e)) {
      return installModuleFor(e).then(() => restart());
    }
    if (shouldDump(e)) {
      console.error(e);
    } else {
      if (!process.env.DEBUG) {
        console.log(
          "Error occurred. For more info, set the DEBUG environment variable (eg set DEBUG=*)."
        );
      }
    }
    process.exit(1);
  }

  function shouldDump(e) {
    return process.env.ALWAYS_DUMP_GULP_ERRORS || probablyNotReportedByGulp(e);
  }

  function probablyNotReportedByGulp(e) {
    var message = (e || "").toString().toLowerCase();
    return ["cannot find module", "referenceerror", "syntaxerror"].reduce(
      (acc, cur) => {
        return acc || message.indexOf(cur) > -1;
      },
      false
    );
  }
}

function looksLikeMissingGulpTasksModule(e) {
  if (!e || !e.message) {
    return false;
  }
  var mod = grokMissingPackageFrom(e.message);
  if (!mod) {
    return false;
  }
  var starter = require("./gulp-tasks/start/package.json");
  return !!starter.devDependencies[mod];
}

function installModuleFor(e) {
  var mod = grokMissingPackageFrom(e.message);
  console.log(`\n\nLooks like you're missing a package required by gulp-tasks: ${mod}\nLet's get that installed and try again (:`);
  return runNpmWith(["install", mod]);
}

function grokMissingPackageFrom(message) {
  var match = message.match("Cannot find module '(.*)'");
  return match && match.length > 1 ? match[1] : null;
}

function restart() {
  return spawn(process.argv[0], process.argv.slice(1), { env: process.env });
}

function runNpmWith(args) {
  var spawn = requireModule("spawn");
  return os.platform() === "win32"
    ? spawn("cmd", ["/c", "npm"].concat(args))
    : spawn("npm", args);
}
