/// <reference path="../node_modules/zarro/types.d.ts" />
(function () {

  const
    gulp = requireModule<Gulp>("gulp"),
    { system } = require("system-wrapper"),
    { rm, folderName, readTextFileLines, writeTextFile, ls, FsEntities } = require("yafs"),
    { ctx } = require("exec-step"),
    which = require("which"),
    env = requireModule<Env>("env");

  gulp.task("generate-docs", async () => {
    const doxygen = await which("doxygen", { nothrow: true });
    if (!doxygen) {
      throw new Error(`doxygen not found in the PATH - is it installed?`);
    }
    const
      folders = await findDocIncludes();
    await updateDoxyfileWithProjectFolders(folders);
    await ctx.exec(
      `generating documentation`,
      async () => {
        await rm("docs");
        await system("doxygen", [], {
          suppressOutput: true
        });
        
      })
  });

  async function updateDoxyfileWithProjectFolders(
    folders: string[]
  ): Promise<void> {
    await ctx.exec(
      `update Doxyfile with project folders`,
      async () => {
        const
          configLines = await readTextFileLines("Doxyfile");
        let inputFound = false;
        for (let i = 0; i < configLines.length; i++) {
          const
            line = configLines[i],
            parts = line.split("=");
          if (parts[0].trim() !== "INPUT") {
            continue;
          }
          configLines[i] = [ parts[0], ` ${folders.join(" ")}` ].join("=");
          inputFound = true;
          break;
        }
        if (!inputFound) {
          throw new Error(`Could not find INPUT directive in Doxyfile`);
        }
        await writeTextFile("Doxyfile", configLines);
      });
  }

  async function findDocIncludes() {
    return (await ctx.exec(
        `finding project folders`, () =>
          ls(".", {
            entities: FsEntities.files,
            fullPaths: false,
            recurse: true,
            match: /\.csproj$/,
            exclude: [
              /.*\.Tests\b/,
              /Artifacts\b/,
              /\bnode_modules\b/,
              /.git\b/,
              /\bbin\b/,
              /\bobj\b/,
              /\.Demo\b/,
              /\bConsumer\b/,
              /\bNugetPackageVersionIncrementer\b/,
              /\bDotNetService\b/,
              /\bServiceWithArgs\b/,
              /\bSpacedService\b/,
              /\bStubbornService\b/,
              /\bTestService\b/,
              /\b_deprecated_\b/,
              /\bDuckTypingInDotnetCore3\b/,
              /\bRandomBuilderPerformanceTest\b/
            ]
          })
      )
    ).map(folderName).concat(["DOCMAIN.md"]);
  }
})();
