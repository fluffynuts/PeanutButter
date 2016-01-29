var gulp = require('gulp');
var exec = requireModule('exec');
var log = requireModule('log');

var format = function(str) {
  return str.replace('\\r', '\r').replace('\\n', '\n');
};

gulp.task('sonar-only', function(done) {
    log.info('Running sonar');
    exec('C:/sonar/sonar-runner-2.4/bin/sonar-runner.bat').then(function() {
        log.info(' >>> HUZZAH <<<');
        done();
    }).catch(function(err) {
        if (!err || !err.error) {
          log.error('Sonar fails and I don\'t know why )\':');
        } else {
          log.error('Sonar fails: ' + err.error.code);   
          log.error('  stdout: ' + format(err.stdout));
          log.error('  stderr: ' + format(err.stderr));
        }
    });
});
