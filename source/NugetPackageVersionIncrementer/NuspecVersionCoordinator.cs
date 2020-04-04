using System;
using System.IO;
using System.Linq;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer
{
    public interface INuspecVersionCoordinator
    {
        void IncrementVersionsUnder(params string[] path);
        Action<string> LogAction { get; set; }
    }

    public class NuspecVersionCoordinator : INuspecVersionCoordinator
    {
        private readonly INuspecFinder _nuspecFinder;
        private readonly INuspecUtilFactory _nuspecUtilFactory;
        public Action<string> LogAction { get; set; }

        public NuspecVersionCoordinator(INuspecFinder nuspecFinder,
                                        INuspecUtilFactory nuspecUtilFactory)
        {
            _nuspecFinder = nuspecFinder ?? throw new ArgumentNullException(nameof(nuspecFinder));
            _nuspecUtilFactory = nuspecUtilFactory ?? throw new ArgumentNullException(nameof(nuspecUtilFactory));
        }

        public void IncrementVersionsUnder(params string[] paths)
        {
            var utils = GetNuspecUtilsForNuspecsUnder(paths);
            IncrementPackageVersionsOn(utils);
            PublishIncrementedPackageVersionsToOtherNuspecsWith(utils);
            PersistUpdatedNuspecsIn(utils);
        }

        private void PersistUpdatedNuspecsIn(INuspecUtil[] utils)
        {
            utils.ForEach(u =>
            {
                u.Persist();
                Log($"{u.PackageId}: {u.OriginalVersion} => {u.Version}");
            });
        }

        private static void PublishIncrementedPackageVersionsToOtherNuspecsWith(INuspecUtil[] utils)
        {
            utils.ForEach(u =>
            {
                var toPublish = utils
                    .Where(p => p != u)
                    .ToArray();
                toPublish.ForEach(p =>
                {
                    u.SetPackageDependencyVersionIfExists(p.PackageId, p.Version);
                });
            });
        }

        private static void IncrementPackageVersionsOn(INuspecUtil[] utils)
        {
            utils.ForEach(u =>
            {
                u.EnsureSameDependencyGroupForAllTargetFrameworks();
                u.IncrementVersion();
            });
        }

        private INuspecUtil[] GetNuspecUtilsForNuspecsUnder(string[] paths)
        {
            paths.ForEach(_nuspecFinder.FindNuspecsUnder);
            var utils = _nuspecFinder.NuspecPaths
                .OrderBy(p => Path.GetFileName(Path.GetDirectoryName(p)))
                .Select(p => _nuspecUtilFactory.LoadNuspecAt(p))
                .ToArray();
            return utils;
        }

        private void Log(string message)
        {
            LogAction?.Invoke(message);
        }
    }

}
