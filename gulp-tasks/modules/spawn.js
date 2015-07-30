// use for spawning actual processes.
// You must use exec if you want to run batch files
var q = require('q');
var child_process = require('child_process');

var defaultOptions = {
    stdio: [process.stdin, process.stdout, process.stderr, 'pipe'],
    cwd: process.cwd()
};

var run = function(executable, args, opts) {
    args = args || [];
    opts = opts || defaultOptions;
    var deferred = q.defer();
    var result = {
        executable: executable,
        args: args
    };

    var child = child_process.spawn(executable, args, opts);
    child.on('error', function(err) {
        result.error = err;
        deferred.reject(result);
    })
    child.on('close', function(code) {
        result.exitCode = code;
        if (code === 0) {
            deferred.resolve(result);
        } else {
            deferred.reject(result);
        }
    });
    return deferred.promise;
}

module.exports = run;
