using System;
using System.Collections.Generic;
using ColorBlockJam.Obstacles;
using NUnit.Framework;

namespace ColorBlockJam.Tests
{
    public sealed class ObstacleCatalogTests
    {
        [Test]
        public void EveryShippedObstacleIsComplete()
        {
            var catalog = TestAssets.LoadOnly<ObstacleCatalog>();
            Assert.IsNotEmpty(catalog.Obstacles);

            var ids = new HashSet<string>();
            var kinds = new HashSet<Type>();
            foreach (var obstacle in catalog.Obstacles)
            {
                Assert.IsNotNull(obstacle, "The catalog has an empty slot.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(obstacle.Id), $"{obstacle.name} has no id.");
                Assert.IsTrue(ids.Add(obstacle.Id), $"The id {obstacle.Id} is used twice, and what the player has seen is kept by id.");
                Assert.IsTrue(kinds.Add(obstacle.GetType()), $"{obstacle.GetType().Name} is introduced twice.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(obstacle.DisplayName), $"{obstacle.name} has no name.");
                Assert.IsFalse(string.IsNullOrWhiteSpace(obstacle.Description), $"{obstacle.name} has no description.");
            }
        }
    }
}
