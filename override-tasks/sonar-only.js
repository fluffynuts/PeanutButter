var gulp = require('gulp');
var exec = requireModule('exec');
var log = requireModule('log');

var format = function(str) {
  return (str || '').replace('\\r', '\r').replace('\\n', '\n');
};

gulp.task('sonar-only', function(done) {
  log.info('Running sonar');
  exec('C:/sonar/sonar-runner-2.4/bin/sonar-runner.bat').then(function() {
    log.info(' >>> HUZZAH <<<');
    done();
  }).catch(function(err) {
    if (!err) {
      log.error('Sonar fails and I don\'t know why )\':');
      done(new Error(err));
    } else {
      log.error('Sonar fails: ' + err.error);
      err.stdout && log.error('  stdout: ' + format(err.stdout));
      err.stderr && log.error('  stderr: ' + format(err.stderr));
      done(err.error || err);
    }
  });
});
