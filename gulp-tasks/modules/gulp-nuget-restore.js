'use strict';
var gutil = require('gulp-util');
var es = require('event-stream');
var fs = require('fs');
var q = require('q');
var testUtilFinder = require('./testutil-finder');
var tmp = require('tmp');
var spawn = require('./spawn');
var log = require('./log');

var PLUGIN_NAME = 'gulp-dotcover';
var DEBUG = true;

var CWD = process.cwd();
function projectPathFor(path) {
  return [CWD, path].join('/');
}

function nugetRestore(options) {
  options = options || { }
  DEBUG = options.debug || false;
  
  var solutionFiles = [];
  
  var stream = es.through(function write(file) {
    if (!file) {
      fail(stream, 'file may not be empty or undefined');
    }
    solutionFiles.push(file);
    this.emit('data', file);
  }, function end() {
    runNugetRestoreWith(this, solutionFiles, options);
  }); 
  return stream;
};

function fail(stream, msg) {
	stream.emit('error', new gutil.PluginError(PLUGIN_NAME, msg));
}
function end(stream) {
	stream.emit('end');
}

function trim() {
	var args = Array.prototype.slice.call(arguments)
	var source = args[0];
	var replacements = args.slice(1).join(',');
	var regex = new RegExp("^[" + replacements + "]+|[" + replacements + "]+$", "g");
	return source.replace(regex, '');
}

function runNugetRestoreWith(stream, solutionFiles, options) {
    var solutions = solutionFiles.map(function(file) {
        return file.path.replace(/\\/g, '/');
    });
    if (solutions.length === 0) {
        return fail(stream, 'No test assemblies defined');
    }
    var nuget = options.nuget || 'nuget.exe';
    var puts = function(str) {
        gutil.log(gutil.colors.yellow(str));
    };
    var opts = {
        stdio: [process.stdin, process.stdout, process.stderr, 'pipe'],
        cwd: process.cwd()
    };

    var deferred = q.defer();
    var final = solutions.reduce(function(promise, item) {
        log.info('Restoring packages for: ' + item);
        var args = [ 'restore', item];
        return promise.then(function() {
            return spawn(nuget, args, opts).then(function() {
                'Packages restored for: ' + item;
            });
        });
    }, deferred.promise);
    final.then(function() {
        end(stream);
    }).catch(function(err) {
        fail(stream, err);
    });

    deferred.resolve();
}

module.exports = nugetRestore;
