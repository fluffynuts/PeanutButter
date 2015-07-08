var gulp = require('gulp');
var fs = require('fs');
var nunit = require('gulp-nunit-runner');
var testUtilFinder = require('./gulp-modules/testutil-finder');

gulp.task('test', function() {
    if (!fs.existsSync('buildreports')) {
        fs.mkdir('buildreports');
    }
    return gulp.src(['**/bin/Debug/**/*.Tests.dll'], { read: false })
                .pipe(nunit({
                    executable: testUtilFinder.latestNUnit({architecture: 'x86'}),
                    options: {
                        result: 'buildreports/nunit-result.xml'
                    }
                }));
});

