const
  gulp = requireModule("gulp"),
  path = require("path"),
  { ls, FsEntities, fileExists } = require("yafs");

gulp.task("list-project-assemblies", async () => {
  const
    ignore = [
      /.*\.Tests.*/,
      /Artifacts/,
      /.*Consumer.*/
    ],
    all = await ls("source", {
      entities: FsEntities.files,
      recurse: true,
      match: [
        /.*\.dll$/
      ]
    });
  const seen = new Set();
  for (const dll of all) {
    const
      filePath = path.join("source", dll),
      parts = dll.split(/[\\\/]/),
      fileName = parts[parts.length - 1],
      assemblyName = fileName.replace(/\.dll$/, ""),
      pathParts = new Set(parts.slice(0, parts.length - 1)),
      shouldIgnore = ignore.reduce(
        (acc, cur) => acc || !!dll.match(cur),
        false
      );
    if (shouldIgnore) {
      continue;
    }
    if (!pathParts.has(assemblyName)) {
      // not in the project path for this asm, ignore it
      continue;
    }
    const xmldoc = filePath.replace(/\.dll$/, ".xml");
    if (!await fileExists(xmldoc)) {
      // no docs, ignore

      continue;
    }
    if (seen.has(fileName)) {
      continue;
    }
    seen.add(fileName);
    console.log(path.join("source", dll));
  }
});
