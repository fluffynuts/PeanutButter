var gulp = require('gulp');
var exec = require('./modules/exec');
var log = require('./modules/log');

gulp.task('sonar-only', function(done) {
    log.info('Running sonar');
    exec('C:/sonar/sonar-runner-2.4/bin/sonar-runner.bat').then(function() {
        done();
    }).catch(function(err) {
    log.error(JSON.stringify(err));
        var code = (err && err.error) ? err.error.code : 'wat';
        var syscall = (err && err.error) ? err.error.syscall : 'unknown system call';
        log.error('Sonar fails: ' + code);   
        log.error('Sonar syscall was: ' + syscall);
        process.exit(1);
    });
});
