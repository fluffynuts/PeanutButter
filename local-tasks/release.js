var
  gulp = require('gulp'),
  runSequence = require('run-sequence'),
  msbuild = require('gulp-msbuild')
  del = require('del'),
  exec = require('../gulp-tasks/modules/exec'),
  findLocalNuget = require('../gulp-tasks/modules/find-local-nuget'),
  es = require('event-stream'),
  commonConfig = {
    toolsVersion: 14.0,
    stdout: true,
    verbosity: 'minimal',
    errorOnFail: true,
    architecture: 'x64'
  };


gulp.task('clean-old-packages', [], function() {
  return del('**/*.nupkg.bak').then(function(paths) {
                  paths.forEach(function(p) {
                    console.log('removed: ' + p);
                  });
                });
});

gulp.task('build-binaries-for-nuget-packages', ['nuget-restore'], function() {
  var config = Object.assign({}, commonConfig);
  config.targets = ['Clean', 'Build'];
  config.configuration = 'BuildForNuget';
  return gulp.src(['**/*.sln', '!**/node_modules/**/*.sln'])
              .pipe(msbuild(config));
});

function processPathsWith(getNugetArgsFor) {
  var files = [];
  var stream = es.through(function(file) {
    if (!file) {
      fail(stream, 'file may not be empty or undefined');
    }
    var filePath = file.history[0];
    files.push(filePath);
    this.emit('data', file);
  }, function() {
    findLocalNuget().then(function(nuget) {
      var promises = [];
      files.forEach(function(pkgPath) {
        var args = getNugetArgsFor(pkgPath);
        if (args) {
          promises.push(exec(nuget, getNugetArgsFor(pkgPath)));
        }
      });
      console.log('...waiting...');
      Promise.all(promises).then(function() {
        stream.emit('end');
      }).catch(function(e) {
        stream.emit('error', new Error(e));
      })
    });
  });
  return stream;
}

function pushNugetPackages() {
  return processPathsWith(function(filePath) {
    return ['push', filePath, '-NonInteractive', '-Source', 'https://www.nuget.org'];
  });
}

var nugetReleaseDir = '.release-packages';
function buildNugetPackages(includeSymbols) {
  return processPathsWith(function(filePath) {
    var args = ['pack', filePath, '-NonInteractive', '-Verbosity', 'Detailed', '-OutputDirectory', nugetReleaseDir];
    if (includeSymbols) {
      args.push('-Symbols');
    }
    return args;
  })
}

gulp.task('build-source-nuget-packages', function() {
  return gulp.src(['**/PeanutButter.TestUtils.MVC.NugetPackage/*.nuspec'])
          .pipe(buildNugetPackages(false));
});

gulp.task('build-binary-nuget-packages', [], function() {
  return gulp.src(['**/source/**/*.nuspec', 
                  '!**/packages/**/*.nuspec', 
                  '!**/PeanutButter.TestUtils.MVC.NugetPackage/**'])
          .pipe(buildNugetPackages(true));
});

gulp.task('clean-nuget-releasedir', function() {
  return del(nugetReleaseDir);
});

gulp.task('build-nuget-packages', ['clean-nuget-releasedir', 'build-binaries-for-nuget-packages'], function(done) {
  runSequence(['build-binary-nuget-packages', 'build-source-nuget-packages'], done);
});

gulp.task('release', ['build-nuget-packages'], function() {
  return gulp.src([nugetReleaseDir + '/*.nupkg', 
                  '!' + nugetReleaseDir + '/*.symbols.nupkg',
                  '!**/packages/**/*.nupkg'])
          .pipe(pushNugetPackages());
});

