using System;
using System.Collections.Generic;
using ColorBlockJam.Boosters;
using ColorBlockJam.Gameplay;
using ColorBlockJam.Obstacles;
using ColorBlockJam.Settings;
using ColorBlockJam.UI.Windows;
using NUnit.Framework;
using UnityEditor;

namespace ColorBlockJam.Tests
{
    public sealed class WindowCatalogTests
    {
        [Test]
        public void EveryWindowHasAPresenterAndAPrefabItCanShow()
        {
            var presenters = new HashSet<Type>();
            foreach (var catalog in TestAssets.LoadAll<WindowCatalog>())
            {
                Assert.IsNotEmpty(catalog.Windows, $"{catalog.name} is empty.");
                foreach (var entry in catalog.Windows)
                {
                    var presenter = entry.PresenterType;
                    Assert.IsNotNull(presenter, $"The presenter '{entry.presenter}' in {catalog.name} does not exist.");
                    Assert.IsTrue(typeof(IWindowPresenter).IsAssignableFrom(presenter) && !presenter.IsAbstract,
                        $"{presenter.Name} is not a window presenter.");
                    Assert.IsTrue(presenters.Add(presenter), $"{presenter.Name} has two windows.");
                    Assert.IsNotNull(entry.prefab, $"{presenter.Name} has no prefab.");
                    Assert.IsInstanceOf(ViewTypeOf(presenter), entry.prefab, $"{presenter.Name} cannot show {entry.prefab.name}.");
                }
            }
        }

        [Test]
        public void EveryWindowCatalogIsInstalled()
        {
            var installed = new SerializedObject(TestAssets.LoadOnly<WindowInstaller>()).FindProperty("catalogs");
            var listed = new HashSet<UnityEngine.Object>();
            for (var i = 0; i < installed.arraySize; i++)
            {
                listed.Add(installed.GetArrayElementAtIndex(i).objectReferenceValue);
            }

            foreach (var catalog in TestAssets.LoadAll<WindowCatalog>())
            {
                Assert.IsTrue(listed.Contains(catalog), $"{catalog.name} is not in the window installer, so its windows never open.");
            }
        }

        [TestCase(typeof(SettingsPopupPresenter))]
        [TestCase(typeof(PauseMenuPresenter))]
        [TestCase(typeof(OutOfTimePopupPresenter))]
        [TestCase(typeof(LevelCompletePopupPresenter))]
        [TestCase(typeof(LevelFailPopupPresenter))]
        [TestCase(typeof(BoosterUnlockPopupPresenter))]
        [TestCase(typeof(ObstacleIntroPopupPresenter))]
        public void EveryWindowTheGameOpensHasAnEntry(Type presenter)
        {
            var found = false;
            foreach (var catalog in TestAssets.LoadAll<WindowCatalog>())
            {
                found |= catalog.TryGetEntry(presenter, out _);
            }

            Assert.IsTrue(found, $"No window catalog shows {presenter.Name}.");
        }

        private static Type ViewTypeOf(Type presenter)
        {
            for (var type = presenter; type != null; type = type.BaseType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(WindowPresenter<,>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            throw new InvalidOperationException($"{presenter.Name} does not derive from {typeof(WindowPresenter<,>).Name}.");
        }
    }
}
