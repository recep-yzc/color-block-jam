using Framework.Core.Persistence;
using Framework.Settings;
using NUnit.Framework;

namespace Framework.Tests
{
    public sealed class SettingsServiceTests
    {
        [Test]
        public void AllSettingsAreOnForANewPlayer()
        {
            var settings = new SettingsService(new InMemoryStorage());

            Assert.IsTrue(settings.IsEnabled(SettingKind.Sound));
            Assert.IsTrue(settings.IsEnabled(SettingKind.Music));
            Assert.IsTrue(settings.IsEnabled(SettingKind.Haptic));
        }

        [Test]
        public void ChangedSettingIsSavedAndRaised()
        {
            var storage = new InMemoryStorage();
            var settings = new SettingsService(storage);
            SettingKind? changed = null;
            settings.Changed += (setting, _) => changed = setting;

            settings.SetEnabled(SettingKind.Music, false);

            Assert.AreEqual(SettingKind.Music, changed);
            Assert.IsFalse(new SettingsService(storage).IsEnabled(SettingKind.Music));
        }

        [Test]
        public void SettingTheSameValueRaisesNothing()
        {
            var settings = new SettingsService(new InMemoryStorage());
            var raised = false;
            settings.Changed += (_, _) => raised = true;

            settings.SetEnabled(SettingKind.Sound, true);

            Assert.IsFalse(raised);
        }
    }
}
