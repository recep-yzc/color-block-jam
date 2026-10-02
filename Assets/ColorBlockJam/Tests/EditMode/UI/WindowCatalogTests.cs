using System;
using System.Collections.Generic;
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
            var guids = AssetDatabase.FindAssets($"t:{nameof(WindowCatalog)}");
            Assert.AreEqual(1, guids.Length);
            var catalog = AssetDatabase.LoadAssetAtPath<WindowCatalog>(AssetDatabase.GUIDToAssetPath(guids[0]));
            Assert.IsNotEmpty(catalog.Windows);

            var presenters = new HashSet<Type>();
            foreach (var entry in catalog.Windows)
            {
                var presenter = entry.PresenterType;
                Assert.IsNotNull(presenter, $"The presenter '{entry.presenter}' does not exist.");
                Assert.IsTrue(typeof(IWindowPresenter).IsAssignableFrom(presenter) && !presenter.IsAbstract,
                    $"{presenter.Name} is not a window presenter.");
                Assert.IsTrue(presenters.Add(presenter), $"{presenter.Name} has two windows.");
                Assert.IsNotNull(entry.prefab, $"{presenter.Name} has no prefab.");
                Assert.IsInstanceOf(ViewTypeOf(presenter), entry.prefab, $"{presenter.Name} cannot show {entry.prefab.name}.");
            }
        }

        private static Type ViewTypeOf(Type presenter)
        {
            for (var type = presenter; type != null; type = type.BaseType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(WindowPresenter<>))
                {
                    return type.GetGenericArguments()[0];
                }
            }

            throw new InvalidOperationException($"{presenter.Name} does not derive from {typeof(WindowPresenter<>).Name}.");
        }
    }
}
