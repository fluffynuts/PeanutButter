var gulp = require('gulp');
var exec = require('./modules/exec');
var log = require('./modules/log');

gulp.task('sonar', ['cover'], function(done) {
    log.info('Running sonar');
    exec('C:/sonar/sonar-runner-2.4/bin/sonar-runner.bat').then(function() {
        done();
    }).catch(function(err) {
        var errorOutput = (err && err.error) ? err.error.stderr : '(no stderr output?)';
        log.error('Sonar fails! Output follows:');   
        log.error(errorOutput.replace('\\r', '\r').replace('\\n', '\n'));
    });
});
