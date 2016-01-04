var gulp = require('gulp');
var exec = require('./modules/exec');
var log = require('./modules/log');

gulp.task('sonar', function(done) {
    log.info('Running sonar');
    exec('C:/sonar/sonar-runner-2.4/bin/sonar-runner.bat').then(function() {
        done();
    }).catch(function(err) {
        log.error('Sonar fails: ' + err.code);   
    });
});
