/// <reference path="../node_modules/zarro/types.d.ts" />
(function () {
    const
        gulp = requireModule<Gulp>("gulp");

    gulp.task("test-zarro", async () => {
        const testZarro = requireModule<TestZarro>("test-zarro");
        await testZarro({
            package: "beta",
            tasks: ["build-nuget-packages"]
        });
    });

    gulp.task("experiment", async() => {
        const system = requireModule<System>("system");
        await system("npm", [ "run", "echo" ]);
    });
})();
