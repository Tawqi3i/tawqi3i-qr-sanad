namespace Tawqi3i.QrSanad.Sanad;

using System.Globalization;
using System.Text;
using Tawqi3i.QrSanad.Common.ViewModels.V1;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

/// <summary>
/// Reads the holder from a MoDEE digital identity card. The card is HTML printed by wkhtmltopdf:
/// every value sits on the row of its label, the Arabic and English full names on the row above the
/// national number, and a rotated watermark crosses the card.
/// </summary>
internal static class IdentityCardReader
{
    private const string NationalNumberLabel = "الرقم الوطني";
    private const int NationalNumberLength = 10;
    private const string DateFormat = "dd/MM/yyyy";

    /// <summary>
    /// Each label as printed, and where its value goes.
    /// </summary>
    private static readonly Dictionary<string, Action<IdentityCardViewModel, string>> Fields = new()
    {
        [NationalNumberLabel] = (card, value) => card.NationalNumber = value,
        ["الجنس"] = (card, value) => card.Gender = value,
        ["فصيلة الدم"] = (card, value) => card.BloodType = value,
        ["تاريخ الميلاد"] = (card, value) => card.BirthDate = ToDate(value),
        ["اسم الأم"] = (card, value) => card.MotherName = value,
        ["رقم القيد"] = (card, value) => card.RegistryNumber = value,
        ["تاريخ الإنتهاء"] = (card, value) => card.ExpiryDate = ToDate(value),
        ["مكان الإقامة"] = (card, value) => card.Residence = value,
        ["رقم الهوية"] = (card, value) => card.CardNumber = value
    };

    /// <summary>
    /// The holder, or null when the PDF is not a digital identity card.
    /// </summary>
    public static IdentityCardViewModel? Read(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);

        return Read(document.GetPage(1).GetWords().ToList());
    }

    public static IdentityCardViewModel? Read(IReadOnlyList<Word> words)
    {
        var rows = ToRows(words.Where(a => a.TextOrientation == TextOrientation.Horizontal));
        var labelRow = rows.FindIndex(row => ToField(row)?.Label == NationalNumberLabel);
        if (labelRow < 1)
        {
            return null;
        }

        var nameRow = rows[labelRow - 1];
        var card = new IdentityCardViewModel
        {
            ArabicName = string.Join(' ', nameRow.Where(IsArabic).OrderByDescending(a => a.BoundingBox.Right).Select(ToLogicalOrder)),
            EnglishName = string.Join(' ', nameRow.Where(a => !IsArabic(a)).OrderBy(a => a.BoundingBox.Left).Select(a => a.Text))
        };

        foreach (var field in rows.Select(ToField).OfType<Field>())
        {
            Fields[field.Label](card, field.Value);
        }

        return IsNationalNumber(card.NationalNumber) && card.ArabicName.Length > 0 && card.EnglishName.Length > 0 ? card : null;
    }

    /// <summary>
    /// Groups the words into lines, top first. A label and its value are in different fonts, so their
    /// baselines differ slightly; words whose middles are within half a word height share a line.
    /// </summary>
    private static List<List<Word>> ToRows(IEnumerable<Word> words)
    {
        var rows = new List<List<Word>>();
        foreach (var word in words.OrderByDescending(a => a.BoundingBox.Centroid.Y))
        {
            var row = rows.LastOrDefault();
            if (row != null && row[0].BoundingBox.Centroid.Y - word.BoundingBox.Centroid.Y < row[0].BoundingBox.Height / 2)
            {
                row.Add(word);
            }
            else
            {
                rows.Add([word]);
            }
        }

        return rows;
    }

    /// <summary>
    /// The label and value of a line, read right to left, or null when the line has no known label or no value.
    /// </summary>
    private static Field? ToField(List<Word> row)
    {
        var text = string.Join(' ', row.OrderByDescending(a => a.BoundingBox.Right).Select(a => IsArabic(a) ? ToLogicalOrder(a) : a.Text));
        var label = Fields.Keys.FirstOrDefault(a => text.StartsWith(a + ' ', StringComparison.Ordinal));

        return label == null ? null : new Field(label, text[(label.Length + 1)..]);
    }

    /// <summary>
    /// The PDF holds Arabic as positioned presentation forms. Reading the letters right to left and
    /// normalising them gives the plain text.
    /// <para>
    /// wkhtmltopdf draws a lam-alef ligature as two glyphs that are missing from the font's ToUnicode
    /// map, so PdfPig returns their raw glyph ids instead of letters. The glyph shape tells them apart:
    /// the lam reaches left under the alef, which follows it, and a hamza sits above or below the alef.
    /// The bounds are in ems, from the Segoe UI glyphs.
    /// </para>
    /// </summary>
    private static string ToLogicalOrder(Word word)
    {
        var text = new StringBuilder();
        var afterLam = false;

        foreach (var letter in word.Letters.OrderByDescending(a => a.StartBaseLine.X))
        {
            var isLam = !IsArabic(letter.Value) && Ems(letter.GlyphRectangle.Left - letter.StartBaseLine.X, letter) < -0.15;
            text.Append(isLam ? "ل" : afterLam ? ToAlef(letter) : letter.Value);
            afterLam = isLam;
        }

        return text.ToString()
            .Normalize(NormalizationForm.FormKC)
            .Replace('ی', 'ي')
            .Replace('ھ', 'ه');
    }

    private static string ToAlef(Letter letter) =>
        Ems(letter.GlyphRectangle.Top - letter.StartBaseLine.Y, letter) > 0.62 ? "أ"
            : Ems(letter.GlyphRectangle.Bottom - letter.StartBaseLine.Y, letter) < -0.1 ? "إ"
            : "ا";

    private static double Ems(double length, Letter letter) => length / letter.PointSize;

    private static DateOnly? ToDate(string value) =>
        DateOnly.TryParseExact(value, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;

    private static bool IsNationalNumber(string text) =>
        text.Length == NationalNumberLength && text.All(char.IsAsciiDigit);

    private static bool IsArabic(Word word) => IsArabic(word.Text);

    private static bool IsArabic(string text) =>
        text.Any(a => a is >= '؀' and <= 'ۿ' or >= 'ﭐ' and <= '﷿' or >= 'ﹰ' and <= 'ﻼ');

    private sealed record Field(string Label, string Value);
}
