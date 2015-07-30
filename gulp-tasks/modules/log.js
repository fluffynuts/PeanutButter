var gutil = require('gulp-util');
var Logger = function() {
};
Logger.prototype = {
    debug: function(message) {
        this._print(message, 'grey');
    },
    info: function(message) {
        this._print(message, 'yellow');
    },
    warning: function(message) {
        this._print(message, 'red');
    },
    error: function(message) {
        this._print(message, 'red', 'bold');
    },
    fail: function(message) {
        this.error(message);
    },
    ok: function(message) {
        this._print(message, 'green');
    },
    notice: function(message) {
        this._print(message, 'cyan');
    },
    _print: function() {
        var message = arguments[0];
        var styles = [];
        for (var i = 1; i < arguments.length; i++) {
            styles.push(arguments[i]);  // because arguments is an object, not an array...
        }
        var styleFunction = styles.reduce(function(acc, cur) {
            var fn = gutil.colors[cur];
            if (fn === undefined) {
                return acc;
            }
            return function(s) {
                return fn(acc(s));
            };
        }, function(s) { return s; });
        gutil.log(styleFunction(message));
    }
};
var logger = new Logger();
module.exports = logger;
