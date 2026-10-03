using ColorBlockJam.Level;
using ColorBlockJam.Obstacles;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Tests
{
    public sealed class ObstacleRulesTests
    {
        private ObstacleIntroductions introductions;
        private ObstacleDefinition arrow;
        private ObstacleDefinition ice;
        private ObstacleDefinition hole;
        private ObstacleDefinition[] obstacles;

        [SetUp]
        public void SetUp()
        {
            introductions = new ObstacleIntroductions(new InMemoryStorage());
            arrow = Create("arrow", ObstacleKind.ArrowBlock);
            ice = Create("ice", ObstacleKind.Ice);
            hole = Create("hole", ObstacleKind.Hole);
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
            CollectionAssert.IsEmpty(ObstacleRules.Pending(obstacles, Level(new BlockData()), introductions));
        }

        [Test]
        public void EachObstacleInTheLevelIsIntroducedInCatalogOrder()
        {
            var level = Level(new BlockData { ice = 2 }, new BlockData { axis = BlockAxis.Vertical });
            level.holes = new[] { new CellData() };

            CollectionAssert.AreEqual(new[] { arrow, ice, hole }, ObstacleRules.Pending(obstacles, level, introductions));
        }

        [Test]
        public void AnObstacleIsIntroducedOnlyOnce()
        {
            var level = Level(new BlockData { ice = 1 });
            introductions.MarkIntroduced(ice);

            CollectionAssert.IsEmpty(ObstacleRules.Pending(obstacles, level, introductions));
        }

        private static LevelData Level(params BlockData[] blocks)
        {
            return new LevelData { blocks = blocks };
        }

        private static ObstacleDefinition Create(string id, ObstacleKind kind)
        {
            var obstacle = ScriptableObject.CreateInstance<ObstacleDefinition>();
            var fields = new SerializedObject(obstacle);
            fields.FindProperty("id").stringValue = id;
            fields.FindProperty("kind").enumValueIndex = (int)kind;
            fields.ApplyModifiedPropertiesWithoutUndo();
            return obstacle;
        }
    }
}
