/*
  Overriding orIn your own source tree:

  If you don't want to import npm scripts as gulp tasks or perhaps you'd
  just like to define a different behavior, create one of the following,
  parallel to gulp-tasks:
  - override-tasks/import-npm-tasks.js
  - local-tasks/import-npm-tasks.js

  If you just want to disable the functionality, an empty file will do
*/
const path = require("path"),
    debug = require("debug")("import-npm-tasks"),
    fs = require("fs"),
    search = ["override-tasks", "local-tasks"];
module.exports = function () {
    let overridden = false;
    search.forEach(function (dir) {
        const lookFor = path.join("..", dir, __filename);
        if (fs.existsSync(lookFor)) {
            console.log("overriding gulp-npm-run default behaviour with: ", lookFor);
            require(lookFor);
            overridden = true;
        }
    });
    if (!overridden) {
        debug("importing npm scripts as gulp tasks");
        require("gulp-npm-run")(requireModule("gulp-with-help"));
        debug("-> done");
    }
};
