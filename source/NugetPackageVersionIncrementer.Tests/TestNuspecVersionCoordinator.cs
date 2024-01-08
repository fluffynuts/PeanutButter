using System;
using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

// ReSharper disable PossibleMultipleEnumeration

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecVersionCoordinator
    {
        [TestCase("nuspecFinder", typeof(INuspecFinder))]
        [TestCase("nuspecUtilFactory", typeof(INuspecUtilFactory))]
        public void Construct_GivenNullParameter_ShouldThrowANE_(string parameter, Type parameterType)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            ConstructorTestUtils.ShouldExpectNonNullParameterFor<NuspecVersionCoordinator>(parameter, parameterType);

            //---------------Test Result -----------------------
        }

        [Test]
        public void Type_ShouldImplement_INuspecVersionCoordinator()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(NuspecVersionCoordinator);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<INuspecVersionCoordinator>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void IncrementVersionsUnder_GivenOneFolderPath_ShouldGivePathToNuspecFinder()
        {
            //---------------Set up test pack-------------------
            var finder = Substitute.For<INuspecFinder>();
            var sut = Create(finder);
            var path = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(path);

            //---------------Test Result -----------------------
            finder.Received(1).FindNuspecsUnder(path);
        }

        [Test]
        public void IncrementVersionsUnder_GivenMultipleFolderPaths_ShouldGiveThemAllToNuspecFinder()
        {
            //---------------Set up test pack-------------------
            var finder = Substitute.For<INuspecFinder>();
            var sut = Create(finder);
            var path1 = RandomValueGen.GetRandomString();
            var path2 = RandomValueGen.GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(path1, path2);

            //---------------Test Result -----------------------
            finder.Received(1).FindNuspecsUnder(path1);
            finder.Received(1).FindNuspecsUnder(path2);
        }

        [Test]
        public void IncrementVersionsUnder_ShouldCreateOneNuspecUtilPerFoundNuspecPath()
        {
            //---------------Set up test pack-------------------
            var finder = Substitute.For<INuspecFinder>();
            var factory = Substitute.For<INuspecUtilFactory>();
            var path1 = RandomValueGen.GetRandomString();
            var nuspecPaths = RandomValueGen.GetRandomCollection<string>(3, 5);
            finder.NuspecPaths.Returns(nuspecPaths);
            var sut = Create(finder, factory);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(path1);

            //---------------Test Result -----------------------
            nuspecPaths.ForEach(p => factory.Received().LoadNuspecAt(p));
        }

        [Test]
        public void IncrementVersionsUnder_ShouldIncrementVersionWithAllUtils()
        {
            //---------------Set up test pack-------------------
            var finder = Substitute.For<INuspecFinder>();
            var factory = Substitute.For<INuspecUtilFactory>();
            var utils = new List<INuspecUtil>();
            factory.LoadNuspecAt(Arg.Any<string>())
                .Returns(_ =>
                {
                    var util = Substitute.For<INuspecUtil>();
                    utils.Add(util);
                    return util;
                });
            var path1 = RandomValueGen.GetRandomString();
            var nuspecPaths = RandomValueGen.GetRandomCollection<string>(3, 5);
            finder.NuspecPaths.Returns(nuspecPaths);
            var sut = Create(finder, factory);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(path1);

            //---------------Test Result -----------------------
            nuspecPaths.ForEach(p => factory.Received().LoadNuspecAt(p));
            utils.ForEach(p => p.Received().IncrementVersion());
        }

        [Test]
        public void IncrementVersionsUnder_ShouldTellEachUtilToSetPackageDependencyIfExists_ForEachOtherNuspecUtil()
        {
            //---------------Set up test pack-------------------
            var path1 = RandomValueGen.GetRandomFileName();
            var path2 = RandomValueGen.GetRandomFileName();
            var util1 = CreateRandomNuspecUtil();
            var util2 = CreateRandomNuspecUtil(util1.PackageId);
            var factory = Substitute.For<INuspecUtilFactory>();
            factory.LoadNuspecAt(path1).Returns(util1);
            factory.LoadNuspecAt(path2).Returns(util2);
            var finder = Substitute.For<INuspecFinder>();
            finder.NuspecPaths.Returns(new[] { path1, path2 });
            var sut = Create(finder, factory);

            //---------------Assert Precondition----------------
            Expect(util1.PackageId)
                .Not.To.Equal(util2.PackageId);

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(RandomValueGen.GetRandomString());

            //---------------Test Result -----------------------
            util1.Received(1).SetPackageDependencyVersionIfExists(util2.PackageId, util2.Version);
            util2.Received(1).SetPackageDependencyVersionIfExists(util1.PackageId, util1.Version);

            var packageId = util1.PackageId;
            var version = util1.Version;
            util1.DidNotReceive().SetPackageDependencyVersionIfExists(packageId, version);
            packageId = util2.PackageId;
            version = util2.Version;
            util2.DidNotReceive().SetPackageDependencyVersionIfExists(packageId, version);
        }

        [Test]
        public void IncrementVersionsUnder_ShouldPersistAfterDoingIncrements_ForEachOtherNuspecUtil()
        {
            //---------------Set up test pack-------------------
            var path1 = RandomValueGen.GetRandomFileName();
            var path2 = RandomValueGen.GetRandomFileName();
            var util1 = CreateRandomNuspecUtil();
            var util2 = CreateRandomNuspecUtil(util1.PackageId);
            var packageId1 = util1.PackageId;
            var packageId2 = util2.PackageId;
            var version1 = util1.Version;
            var version2 = util2.Version;
            var factory = Substitute.For<INuspecUtilFactory>();
            factory.LoadNuspecAt(path1).Returns(util1);
            factory.LoadNuspecAt(path2).Returns(util2);
            var finder = Substitute.For<INuspecFinder>();
            finder.NuspecPaths.Returns(new[] { path1, path2 });
            var sut = Create(finder, factory);

            //---------------Assert Precondition----------------
            Expect(util1.PackageId)
                .Not.To.Equal(util2.PackageId);

            //---------------Execute Test ----------------------
            sut.IncrementVersionsUnder(RandomValueGen.GetRandomString());

            //---------------Test Result -----------------------
            Received.InOrder(() =>
            {
                util1.EnsureSameDependencyGroupForAllTargetFrameworks();
                util1.IncrementVersion();
                util2.EnsureSameDependencyGroupForAllTargetFrameworks();
                util2.IncrementVersion();

                util1.SetPackageDependencyVersionIfExists(packageId2, version2);
                util2.SetPackageDependencyVersionIfExists(packageId1, version1);

                util1.Persist();
                util2.Persist();
            });
        }

        private INuspecUtil CreateRandomNuspecUtil(string existingPackageId = null)
        {
            var packageId = RandomValueGen.GetAnother(existingPackageId, () => RandomValueGen.GetRandomString(10, 20));
            var version = CreateRandomVersion();
            var util = Substitute.For<INuspecUtil>();
            util.PackageId.Returns(packageId);
            util.Version.Returns(version);
            return util;
        }

        private string CreateRandomVersion()
        {
            return string.Join(".", RandomValueGen.GetRandomCollection<int>(3,3));
        }


        private INuspecVersionCoordinator Create(INuspecFinder finder = null,
                                                 INuspecUtilFactory factory = null)
        {
            return new NuspecVersionCoordinator(finder ?? Substitute.For<INuspecFinder>(),
                                                factory ?? Substitute.For<INuspecUtilFactory>());
        }
    }
}
