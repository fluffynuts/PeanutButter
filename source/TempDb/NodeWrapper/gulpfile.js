// noinspection JSUnusedLocalSymbols
const
    tasksFolder = "gulp-tasks",
    requireModule = (mod) => {
        return require(`./${tasksFolder}/modules/${mod}`);
    },
    gulp = requireModule("gulp-with-help"),
    requireDir = require("require-dir");

global["requireModule"] = requireModule;

requireDir(tasksFolder);
