/*
 * Welcome new user! To get started, run this file like so
 * > node gulpfile.js
 * to generate a very basic packages.json. You will be instructed to:
 * > npm install
 * after that, you can:
 * > gulp
 * 
 * To add or change tasks, do so one task per file in the gulp-tasks folder
 */
var fs = require('fs');
if (!fs.existsSync('package.json')) {
    fs.writeFileSync('package.json', '{\n"devDependencies":\n{"event-stream": "^3.3.1",\n"gulp": "^3.9.0",\n"gulp-msbuild": "^0.2.11",\n"gulp-nunit-runner": "^0.4.1",\n"gulp-util": "^3.0.6",\n"require-dir": "^0.3.0",\n"run-sequence": "^1.1.0",\n"through2": "^2.0.0",\n"tmp": "0.0.26"\n,"q": "^1.4.1"\n}\n}');
    console.log('a package.json has been made for you. Please \'npm install\' and run \'gulp\'');
    process.exit();
}
require('require-dir')('gulp-tasks');
