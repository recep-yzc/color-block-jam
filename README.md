# Color Block Jam — Vertical Slice

Color Block Jam'in Unity 2022.3.62f2 (URP) ile, 1080×1920 dikey ekran için yapılmış bir vertical slice'ı. Zorunlu kapsamın tamamı ve dört opsiyonel maddenin dördü de yapıldı: çözülemeyen bir level kaydedilmeden önce uyarı, çalışan booster'lar, buz ve ok bloklar. Delikler, AUTO ve level üretici ek olarak var. Android build'i repo kökündeki [`ColorBlockJam.apk`](ColorBlockJam.apk) dosyası.

## Projeyi açma ve çalıştırma

1. Projeyi **Unity 2022.3.62f2** ile açın. Paketler `Packages/manifest.json` üzerinden yüklenir.
2. `Assets/ColorBlockJam/Scenes/Splash.unity` sahnesini açıp **Play** butonuna basın. Game view'u 1080×1920 ya da 1080×2400 (20:9) yapın.
3. Build için *File › Build Settings* kullanılır: sahneler (Splash, Main, Gameplay) listede hazır, Android IL2CPP ve ARM64 ile derlenir.
4. *Window › General › Test Runner* 97 EditMode ve 3 PlayMode testini çalıştırır.
5. *Edit › Clear All PlayerPrefs* ilerlemeyi, coinleri, booster'ları ve görülen engelleri sıfırlar.

**Nasıl oynanır.** Bir bloğu sürükleyip kendi rengindeki bir kapıya itin; şeklinin tamamı kapıdan geçebiliyorsa tahtadan çıkar. Süre bitmeden tahtayı temizleyin. Ok bloklar yalnızca okları yönünde hareket eder. Donmuş bir blok, üzerindeki sayı kadar başka blok çıkana kadar hareket edemez. Delikler tahtadan çıkarılmış hücrelerdir. Booster'lar 2, 4, 6 ve 8. levellerde açılır: Freeze süreyi 10 saniye durdurur, Hammer bir bloğu kırar, Rocket bir satırı, Vacuum ise bir rengin bütün bloklarını temizler. **AUTO** leveli solver'a bitirtir. Editörde ve development build'lerde → ve ← tuşları leveller arasında geçiş yapar.

## Editörü açma ve level yapma

Editör **Color Block Jam › Level Editor** menüsünden, bir level dosyasına (`Systems/Level/Data/Levels/LevelNNN.json`) çift tıklayarak ya da `LevelCatalog.asset` üzerindeki **Open Level Editor** butonuyla açılır. Solda oynanış sırasıyla leveller, ortada tahta, sağda ayarlar ve araçlar bulunur. Kontrollerin üzerine gelince açıklamaları (tooltip) görünür.

1. **New** butonuna basın ya da soldan bir level seçin.
2. **Width**, **Height**, **Time** ve **Difficulty** değerlerini girin.
3. Bir renk (1–0 tuşları) ve bir araç seçin: **Draw** (hücrelerin üzerinden sürükleyerek istenen şekilde blok çizer), **Stamp** (hazır bir şekil koyar), **Door** (duvar boyunca tıklayarak ya da sürükleyerek kapı koyar), **Move**, **Erase** ya da **Hole**. Sağ tık her araçta siler; Ctrl+Z geri alır, Ctrl+Y yineler.
4. Bir bloğa tıklayıp onu ok blok (**Moves**) ya da donmuş blok (**Ice**) yapın.
5. **Check**, kapısı olmayan bir renk ya da hiçbir kapıya sığmayan bir blok gibi hataları listeler ve arka planda solver'ı çalıştırır. Levelin çözülüp çözülemediğini, kaç hamlede çözüldüğünü ve hangi zorlukta olduğunu söyler; ◀ ▶ ile çözüm adım adım izlenir.
6. **Generate**, seçilen zorlukta çözülebilir yeni bir level üretir. Ayarları `GeneratorPresets.asset` dosyasındadır.
7. **Save** levelin dosyasının üzerine yazar, **Save As New Level** kataloğun sonuna yeni bir dosya ekler. Hatası olan, çözülemeyen ya da çözülebilirliği henüz kanıtlanmamış bir level kaydedilmeden önce editör uyarır.
8. **▶ Play** leveli oyunda açar. Test, oyun kaydının bir kopyası üzerinde oynanır ve bitince değişiklikler atılır.

Editör ve oyun tek bir formatı, levelin JSON dosyasını (`LevelData`) okur. `LevelCatalog.asset` bu dosyaları oynanış sırasıyla listeler.

## Mimari kararlar ve nedenleri

| Karar | Neden |
|---|---|
| **Her sistem için bir klasör ve bir assembly**, `Assets/ColorBlockJam/Systems` altında: Core, Boot, UI, Navigation, Settings, Pooling, Economy, Progression, Level, Gameplay, Boosters, Obstacles, Home, LevelEditor, Rendering. | Bir sistem tek bir yerde okunur, değiştirilir ya da kaldırılır. Referanslar tek yönlüdür; genel sistemler (Core, UI, Navigation, Settings, Pooling) oyun hakkında hiçbir şey bilmez. |
| **Veri, kurallar ve görünüm ayrı.** `Level.Data` (level formatı), `Gameplay.Logic` (tahta, hareket, solver, süre) ve editörün `LevelEditor.Authoring` assembly'si (üretici, kontroller, zorluk puanı) Unity'ye referans vermez. | Oyun, editör, testler ve arka plan thread'leri her kuralın tek bir uygulamasını kullanır; örneğin bir bloğun kapıdan ne zaman çıkabileceği tek bir yerde yazılıdır. |
| **VContainer**, uygulama servisleri için bir kök scope, her sahne için bir scope ve her level için bir alt scope kurar. | Singleton ya da statik durum yoktur. Restart ve Next levelin scope'unu kapatıp yenisini kurar; önceki denemeden hiçbir şey kalmaz. |
| **UI'da MVP.** View'lar yalnızca gösterir ve olayları bildirir, presenter'lar düz C# sınıflarıdır. Her sistem popup'larını bir pencere kataloğunda listeler ve `ShowAsync` oyuncunun seçimini döndürür. | Presenter'lar sahne olmadan test edilir ve bir popup onu açan leveli tanımaz. |
| **İçerik ve his ScriptableObject asset'leridir**: config'ler, booster'lar, engeller, buton tepkileri, geçişler ve toggle görünümleri. | Tasarımcı bunları kod yazmadan ayarlar; yeni bir booster ya da engel bir sınıf ve bir asset'ten ibarettir. |
| **Fiziksiz hareket.** Sürüklenen blok yolunu baştan sona tarar, değdiği yüzey boyunca kayar ve bevel'li köşelerin etrafından kavis çizerek döner. | Hareket kesin ve deterministiktir: bloklar asla iç içe geçmez ve sürükleme her karede bellek ayırmaz (bunu bir test kontrol eder). |
| **Tek bir solver**: blok hamleleri üzerinde genişlik öncelikli arama, arka plan thread'inde çalışır. | Oyun takılan tahtayı fark eder, editör de aynı kodla levellerin çözülebilir olduğunu kanıtlar ve zorluklarını puanlar; bunu yaparken kare düşürmez. |
| **Az çizim çağrısı (draw call).** Her blok ve tahta tek bir mesh; renkler vertex'lerde, materyaller ortak, tek bir toon shader, pool'dan gelen partiküller ve bir sprite atlası. | SRP Batcher ile bir level birkaç çizim çağrısında çizilir; bu da orta seviye telefonlara uygundur. |
| **Shader'lar yükleme ekranında hazırlanır.** `ShaderVariants` koleksiyonu oyunun çizdiği varyantları Graphics Settings üzerinden önceden yükler. Splash sırasında `BoardWarmup` oyunun kendi koduyla kurduğu küçük bir tahtayı (bloklar, tutulan bloğun outline'ı, buz ve sayacı, kapı, zemin, partikül, gölge) splash kamerasından önce çizilen bir kamerayla, `WindowWarmup` da pencerelerin kullandığı her materyali splash'ın arkasında birer kez çizer. URP'nin kullanmadığı Built-in ve video shader'ları build'e girmez. | Vulkan bir shader'ı ancak ilk çiziminde, çizildiği hedefe ve vertex düzenine göre tamamlar. Bu iş yükleme ekranında yapıldığı için ilk blok tutuşunda, ilk patlamada ya da ilk popup'ta takılma olmaz. |
| **Kayıtlar `IKeyValueStorage` üzerinden yapılır** (PlayerPrefs; uygulama odağı kaybettiğinde ya da kapanırken diske yazılır). | Depolama oyuna dokunmadan değiştirilebilir; editörden açılan test oyunu kayıtlarını bellekte tutar. |

| Paket | Neden |
|---|---|
| URP 14 | SRP Batcher ile mobile uygun render. |
| Input System | Fare ve dokunma için tek bir girdi yolu. |
| VContainer | İç içe scope'ları olan hızlı dependency injection. |
| UniTask | Popup'lar, beklemeler ve arka plandaki solver için bellek ayırmayan async/await. |
| LitMotion (Burst ve Collections ile) | UI ve tahta animasyonları için bellek ayırmayan tween'ler. |
| TextMesh Pro | Kenar çizgisi olan net yazılar. |
| 2D feature set, Device Simulator devices | 9-slice kenarları için Sprite Editor, yerleşimi kontrol etmek için telefon ekranları. |

## Belirsiz maddelerde verilen kararlar

- **Leveller.** Orijinal oyunun levelleri verilmediği için leveller yeni tasarlandı. 50 level var, hepsi editörle yapıldı: 1–3 eğitim leveli olarak elle, diğerleri editörün üreticisiyle. Her birinin çözülebilir olduğu kanıtlandı ve zorluğu hamle sayısına göre belirlendi.
- **Süre.** Bir bloğa ilk dokunuşta başlar. Süre sıfırlanınca level durur ve **Out of Time!** 100 coin karşılığında 20 saniye önerir; oyuncu reddederse fail popup'ı açılır.
- **Fail.** Fail popup'ı (**Retry**, **Home**) süre bittiğinde ya da tahta artık temizlenemez olduğunda açılır.
- **Pause**, ayarları bir **HOME** butonuyla birlikte açar; ayarlar kapanınca level devam eder.
- **Restart** leveli verisinden yeniden kurar. Deneme sırasında harcanan coinler ve booster'lar geri gelmez.
- **Coinler.** Oyuncu 100 coinle başlar ve bitirdiği level için zorluğuna göre 10, 20, 30 ya da 50 coin kazanır. Coinlerle ek süre ve booster alınır.

## Bilinen sorunlar ve eksik kısımlar

- **Yer tutucular** (case'in izin verdiği gibi): can göstergesi 5 gösterir ve level kaybetmek can götürmez. Shop ve Collection sekmeleri yalnızca başlığı olan sayfalar açar, kilitli sekmeler bir şey yapmaz. Profil, artı, dil, destek, yasal metinler ve satın alımları geri yükleme butonları yalnızca dokunma tepkisi verir.
- Oyundaki leveller hiç takılmaz: hepsinin çözülebilir olduğu kanıtlandı ve oyun sırasında çözülebilirlik değişmez. Takılınca açılan fail popup'ını görmek için editörde çözülemeyen bir level yapıp **▶ Play** butonuna basın.
- Çok büyük özel bir levelde **Check**, kesin bir cevap yerine "No solution found within 30000 positions" uyarısı verebilir.
- 50. levelden sonra leveller 1. levelden yeniden başlar, ana ekrandaki level numarası ise artmaya devam eder.
- Editörden **▶ Play** ile açılan bir testte **Continue**, **Retry** ve **Home › Play** test edilen levele döner.
- Bir booster'ın açıklaması düz metindir; sayılarını değiştirirken metni de elle değiştirmek gerekir.
- UI sprite'ları netlik için Android'de sıkıştırılmadan tutulur; bu yüzden arka planlar bellekte yaklaşık 25 MB yer kaplar.

## Verilen asset'ler hakkında geri bildirim

- "Toggle off" sprite'ı yoktu; `btn_toggle_off.png` eklendi.
- `BlockParts.fbx` ve `Arrows.fbx` XY düzleminde ve −Z'ye bakacak şekilde modellenmiş, `WallAndDoor.fbx` ve `GroundGrid.fbx` ise Y-up. Duvar, köşe ve kapı modelleri ayrıca baş aşağı, duvar da çeyrek tur dönük; `BoardArt` üzerindeki *Model turns* ayarı her birini düzeltir.
- Modeller birbirine kusursuz oturmuyor; parçaların birleştiği yerlerde ince boşluklar kalıyor. Bu boşluklar kötü görünmesin diye URP asset'inde kenar yumuşatma (4x MSAA) açık.
- `WallAndDoor.fbx` eksik bir gömülü dokuya referans veriyor, `corner_4` ise kullanılmayan bir blend shape içeriyor.
- Booster ikonları (1024×1024) ve buz dokusu (2048×2048) ekrandaki boyutlarından çok büyük.
- Yumuşak gölgeler ve parlamalar otomatik 9-slice kenarlarını güvenilmez kılıyor; kenarlar elle ayarlandı.
- Engel ikonları verilmedi. Referans ekranlardaki Watch Ad butonu, coin yığını ve fail teklifi de verilmediği için bunlar yapılmadı.

## Kullanılan LLM araçları ve kullanıldıkları kısımlar

**Claude Code** (Anthropic, Claude Opus 5.5 modeli) baştan sona eşli programlama için kullanıldı. Mimariyi önerdi; C# kodunun çoğunu, shader'ları ve editör araçlarını yazdı; verilen modelleri inceledi; testleri ve commit mesajlarını yazdı ve bu README'nin taslağını hazırladı. Her adımı ben yönlendirdim ve gözden geçirdim.

## Yaklaşık çalışma süresi

Commit geçmişine göre yedi oturumda yaklaşık 25 saat: 28 Eylül 2026 20:10 ile 4 Ekim 2026 16:30 arası.
