using ColorBlockJam.Settings;
using NUnit.Framework;

namespace ColorBlockJam.Tests
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
        public void AChangedSettingIsSaved()
        {
            var storage = new InMemoryStorage();
            var settings = new SettingsService(storage);

            settings.SetEnabled(SettingKind.Music, false);

            Assert.IsFalse(settings.IsEnabled(SettingKind.Music));
            Assert.IsFalse(new SettingsService(storage).IsEnabled(SettingKind.Music));
        }
    }
}
