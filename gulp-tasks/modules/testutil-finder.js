'use strict';
var log = require('./log');
var fs = require('fs');
var programFilesFolder = 'C:/Program Files (x86)';
function isUnstable(folderName) {
    return folderName.indexOf('alpha') > -1 ||
            folderName.indexOf('beta') > -1;
};
function finder(searchFolder, searchBin, options, searchBaseFolder) {
    searchBaseFolder = searchBaseFolder || programFilesFolder;
    var ignoreBetas = options.ignoreBetas === undefined ? true : options.ignoreBetas;
    
    var programFolders = fs.readdirSync(searchBaseFolder);
    var lsearch = searchFolder.toLowerCase();
    var possibles = programFolders.reduce(function(acc, cur) {
        var lcur = cur.toLowerCase();
        if (lcur.indexOf(lsearch) === 0) {
            if (ignoreBetas && isUnstable(lcur)) {
                log.notice('Ingnoring unstable tool at: ' + cur);
                return acc;
            } 
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
            log.debug('Using ' + runner);
            return runner;
        }
    }
    throw 'not found';
}

function findWrapper(func, name) {
    try {
        return func();
    } catch (e) {
        switch (e) {
            case 'no possibles':
                throw 'Can\'t find any installed ' + name;
            case 'not found':
                throw 'Found ' + name + ' folder but no binaries to run )\':';
            default:
                throw e;
        }
    }
}

var testUtilFinder = {
    latestNUnit: function(options) {
        options = options || {};
        var runner = (options.x86 || ((options.platform || options.architecture) === 'x86')) ? '/bin/nunit-console-x86.exe' : '/bin/nunit-console.exe';
        return findWrapper(function() {
            return finder('NUnit', runner, options);        
        });
    },
    latestDotCover: function(options) {
        options = options || {};
        return findWrapper(function() {
            return finder('v', '/bin/dotCover.exe', options, programFilesFolder + '/JetBrains/dotCover');
        });
    }
};
module.exports = testUtilFinder;
