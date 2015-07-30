// MUST use for running batch files
// you can use this for other commands but spawn is better
// as it handles IO better
var q = require('q');
var child_process = require('child_process');

var defaultOptions = {
    cwd: process.cwd(),
    maxBuffer: 1024 * 1024
};

var exec = function(cmd, args, opts) {
    var deferred = q.defer();
    args = args || [];
    opts = opts || defaultOptions;
    child_process.execFile(cmd, args, opts, function(error, stdout, stderr) {
        if (error) {
            return deferred.reject({
                error: error,
                stderr: stderr,
                stdout: stdout
            });
            deferred.resolve(stdout);
        }
    });
    return deferred.promise;
};

module.exports = exec;
