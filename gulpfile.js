/*
 * Welcome new user! To get started, ensure that you have copied
 * the start/packages.json alongside this file, in the root of your repo.
 * Then do:
 * > npm install
 * After that, you can:
 * > node node_modules/gulp/bin/gulp.js
 * OR, install gulp globally:
 * > npm install -g gulp
 * and then you can just:
 * > gulp
 * I HIGHLY recommend using the first method at your build server
 *
 * To add or change tasks, do so one task per file in the gulp-tasks folder
 */
var fs = require('fs'),
    debug = require('debug')('gulpfile');
var gulpTasksFolder = 'gulp-tasks'; // if you cloned elsewhere, you'll need to modify this
global.requireModule = function(module) {
    var modulePath = ['.', gulpTasksFolder, 'modules', module].join('/');
    return require(modulePath);
};
var fs = require('fs');
if (!fs.existsSync('package.json')) {
    ['You\'re nearly there!',
     'Please copy the package.json from the start folder alongside your gulpfile.js',
     'then run `npm install` to install the required packages'].forEach(function(s) {
        console.log(s);
     });
    process.exit(1);
}
try {
    var requireDir = require('require-dir');
    requireDir('gulp-tasks');
    ['override-tasks', 'local-tasks'].forEach(function(dirname) {
        if (fs.existsSync(dirname)) {
            requireDir(dirname);
        }
    });
} catch (e) {
    if (shouldDump(e)) {
      console.error(e);
    } else {
      if (!process.env.DEBUG) {
        console.log("Error occurred. For more info, set the DEBUG environment variable (eg set DEBUG=*).")
      }
    }
    process.exit(1);
}

function shouldDump(e) {
  return process.env.ALWAYS_DUMP_GULP_ERRORS || probablyNotReportedByGulp(e);
}

function probablyNotReportedByGulp(e) {
  var message = (e || "").toString().toLowerCase();
  return [
    "cannot find module",
    "referenceerror",
    "syntaxerror"
  ].reduce((acc, cur) => {
    return acc || message.indexOf(cur) > -1;
  }, false);
}
