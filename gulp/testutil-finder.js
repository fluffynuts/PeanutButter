'use strict';
var fs = require('fs');
var programFilesFolder = 'C:/Program Files (x86)';
function finder(searchFolder, searchBin, searchBaseFolder) {
    searchBaseFolder = searchBaseFolder || programFilesFolder;
    var programFolders = fs.readdirSync(searchBaseFolder);
    var lsearch = searchFolder.toLowerCase();
    var possibles = programFolders.reduce(function(acc, cur) {
        if (cur.toLowerCase().indexOf(lsearch) === 0) {
            acc.push(cur);
        }
        return acc;
    }, []);
    if (possibles.length === 0) {
        throw 'no possibles';
    }
    possibles.sort();
    for (var i = possibles.length-1; i > -1; i--) {
        var runner = [searchBaseFolder, '/', possibles[i], searchBin].join('');
        if (fs.existsSync(runner)) {
            console.log('Using ' + runner);
            return runner;
        }
    }
    throw 'not found';
}

var testUtilFinder = {
    latestNUnit: function() {
        try
        {
            return finder('NUnit', '/bin/nunit-console.exe');
        } catch (e) {
            switch (e) {
            case 'no possibles':
                throw 'Can\'t find any installed NUnit';
            case 'not found':
                throw 'Found NUnit folder, but no binaries?!';
            }
        }
    },
    latestDotCover: function() {
        return finder('v', '/bin/dotCover.exe', programFilesFolder + '/JetBrains/dotCover');
    }
};
module.exports = testUtilFinder;
