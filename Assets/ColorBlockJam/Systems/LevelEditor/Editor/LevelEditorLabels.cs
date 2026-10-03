using UnityEngine;

namespace ColorBlockJam.LevelEditor
{
    internal static class LevelEditorLabels
    {
        public static readonly GUIContent WindowTitle = new("Level Editor");
        public static readonly GUIContent New = new("New", "Boş bir seviye başlatır.");
        public static readonly GUIContent Save = new("Save", "Seviyenin dosyasının üzerine kaydeder.");
        public static readonly GUIContent SaveAsNew = new("Save As New Level", "Kataloğun sonuna yeni bir dosya olarak kaydeder.");
        public static readonly GUIContent Undo = new("Undo", "Geri alır (Ctrl+Z).");
        public static readonly GUIContent Redo = new("Redo", "Yineler (Ctrl+Y).");
        public static readonly GUIContent Check = new("Check", "Seviyenin çözülebilir olup olmadığını bulur.");
        public static readonly GUIContent Play = new("▶ Play", "Bu seviyeyi oyun sahnesinde oynatır. Oyun çalışırken kapalıdır.");
        public static readonly GUIContent MoveUp = new("▲", "Seviyeyi katalogda bir öne alır.");
        public static readonly GUIContent MoveDown = new("▼", "Seviyeyi katalogda bir sonraya alır.");
        public static readonly GUIContent Remove = new("Remove", "Seviyeyi katalogdan çıkarır. Dosyası silinmez.");
        public static readonly GUIContent Width = new("Width", "Yatayda hücre sayısı.");
        public static readonly GUIContent Height = new("Height", "Aşağıdan yukarıya hücre sayısı.");
        public static readonly GUIContent Time = new("Time (seconds)", "Süre sıfıra inince seviye kaybedilir.");
        public static readonly GUIContent Difficulty = new("Difficulty", "Seviyede oyuncuya gösterilir.");
        public static readonly GUIContent BlockColor = new("Color", "Seçili bloğun rengi. Bloğu bu renkteki bir kapıdan çıkar.");
        public static readonly GUIContent Moves = new("Moves", "Serbest ya da tek eksende: ok bloğu. Üstündeki ok hangi yönde gidebildiğini gösterir.");
        public static readonly GUIContent Ice = new("Ice", "Blok donmuş başlar ve bu kadar başka blok çıkana kadar hareket edemez. 0 = buz yok.");
        public static readonly GUIContent DeleteBlock = new("Delete Block", "Seçili bloğu siler (Delete).");
        public static readonly GUIContent Shape = new(string.Empty, "Stamp aracıyla yerleştirilecek şekil.");
        public static readonly GUIContent CheckLevel = new("Check Level", "Seviyenin çözülebilir olup olmadığını arka planda bulur.");
        public static readonly GUIContent StepBack = new("◀", "Çözümde bir adım geri.");
        public static readonly GUIContent StepForward = new("▶", "Çözümde bir adım ileri.");
        public static readonly GUIContent GenerateDifficulty = new("Difficulty", "Üretilecek seviyenin zorluğu.");
        public static readonly GUIContent Seed = new("Seed", "Aynı seed her zaman aynı seviyeyi üretir.");
        public static readonly GUIContent RandomSeed = new("🎲", "Rastgele bir seed seçer.");
        public static readonly GUIContent Holes = new("Holes", "Tahtanın içine açılacak delik sayısı. Her delik en az 2x2 hücredir.");
        public static readonly GUIContent EditPresets = new("Edit Presets",
            "Her zorluk için tahta boyunu, blok ve renk sayısını, ok ve buz oranını ve şekilleri tutan ayar asset'ini seçer.");
        public static readonly GUIContent GenerateLevel = new("Generate Level", "Bu zorlukta çözülebilir yeni bir seviye üretir.");
    }
}
