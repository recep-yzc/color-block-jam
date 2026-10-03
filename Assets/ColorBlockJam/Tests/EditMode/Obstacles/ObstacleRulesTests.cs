using ColorBlockJam.Level;
using ColorBlockJam.Obstacles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class ObstacleRulesTests
    {
        private SeenObstacles seen;
        private ObstacleDefinition arrow;
        private ObstacleDefinition ice;
        private ObstacleDefinition hole;
        private ObstacleDefinition[] obstacles;

        [SetUp]
        public void SetUp()
        {
            seen = new SeenObstacles(new InMemoryStorage());
            arrow = Create<ArrowBlockObstacle>("arrow");
            ice = Create<IceObstacle>("ice");
            hole = Create<HoleObstacle>("hole");
            obstacles = new[] { arrow, ice, hole };
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(arrow);
            Object.DestroyImmediate(ice);
            Object.DestroyImmediate(hole);
        }

        [Test]
        public void APlainLevelIntroducesNothing()
        {
            CollectionAssert.IsEmpty(ObstacleRules.Pending(obstacles, Level(new BlockData()), seen));
        }

        [Test]
        public void EachObstacleInTheLevelIsIntroducedInCatalogOrder()
        {
            var level = Level(new BlockData { ice = 2 }, new BlockData { axis = BlockAxis.Vertical });
            level.holes = new[] { new CellData() };

            CollectionAssert.AreEqual(new[] { arrow, ice, hole }, ObstacleRules.Pending(obstacles, level, seen));
        }

        [Test]
        public void AnObstacleIsIntroducedOnlyOnce()
        {
            var level = Level(new BlockData { ice = 1 });
            seen.MarkSeen(ice);

            CollectionAssert.IsEmpty(ObstacleRules.Pending(obstacles, level, seen));
        }

        private static LevelData Level(params BlockData[] blocks)
        {
            return new LevelData { blocks = blocks };
        }

        private static T Create<T>(string id) where T : ObstacleDefinition
        {
            var obstacle = ScriptableObject.CreateInstance<T>();
            var fields = new SerializedObject(obstacle);
            fields.FindProperty("id").stringValue = id;
            fields.ApplyModifiedPropertiesWithoutUndo();
            return obstacle;
        }
    }
}
