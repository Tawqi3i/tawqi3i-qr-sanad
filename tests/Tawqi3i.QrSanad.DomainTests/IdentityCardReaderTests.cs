namespace Tawqi3i.QrSanad.DomainTests;

using Tawqi3i.QrSanad.Sanad;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Graphics.Colors;
using UglyToad.PdfPig.PdfFonts;

/// <summary>
/// Builds the words as PdfPig reads them from a MoDEE identity card. In the Arabic samples, the
/// characters below stand for the lam-alef glyphs that wkhtmltopdf leaves without a Unicode value.
/// </summary>
public class IdentityCardReaderTests
{
    private const double FontSize = 12;
    private const double LetterWidth = FontSize / 2;

    // Glyph bounds in ems (left, bottom, top), measured on a real card. Any other character, such as
    // the '/' in a place of residence, is drawn like an ordinary letter.
    private static readonly Dictionary<char, (double Left, double Bottom, double Top)> UnmappedGlyphs = new()
    {
        ['#'] = (-0.23, -0.01, 0.56), // lam
        ['$'] = (0.03, 0.01, 0.50), // alef
        ['6'] = (0.02, 0.01, 0.70), // alef with hamza above
        ['8'] = (0.03, -0.19, 0.50) // alef with hamza below
    };

    private static readonly FontDetails Font = new("SegoeUIBold", true, 700, false);

    [Test]
    public void ReadsEveryField()
    {
        var card = IdentityCardReader.Read(Card("محمد احمد علي", "MOHAMMAD AHMAD ALI", "2000000001"))!;

        Assert.That(card.NationalNumber, Is.EqualTo("2000000001"));
        Assert.That(card.ArabicName, Is.EqualTo("محمد احمد علي"));
        Assert.That(card.EnglishName, Is.EqualTo("MOHAMMAD AHMAD ALI"));
        Assert.That(card.Gender, Is.EqualTo("ذكر"));
        Assert.That(card.BloodType, Is.EqualTo("O+"));
        Assert.That(card.BirthDate, Is.EqualTo(new DateOnly(2000, 7, 16)));
        Assert.That(card.MotherName, Is.EqualTo("سلمى"));
        Assert.That(card.RegistryNumber, Is.EqualTo("013/049"));
        Assert.That(card.ExpiryDate, Is.EqualTo(new DateOnly(2032, 7, 20)));
        Assert.That(card.Residence, Is.EqualTo("حلاوه/قصبه عجلون"));
        Assert.That(card.CardNumber, Is.EqualTo("ABC12345"));
    }

    [Test]
    public void LeavesAMissingFieldNull()
    {
        var words = Card("محمد", "MOHAMMAD", "2000000001")
            .Where(a => a.BoundingBox.Bottom > 300)
            .ToList();

        var card = IdentityCardReader.Read(words)!;

        Assert.That(card.NationalNumber, Is.EqualTo("2000000001"));
        Assert.That(card.CardNumber, Is.Null);
    }

    [TestCase("ب#$ل", "بلال")]
    [TestCase("ا#6مين", "الأمين")]
    [TestCase("عبدا#8له", "عبدالإله")]
    public void RepairsTheLamAlefLigature(string drawn, string expected)
    {
        var card = IdentityCardReader.Read(Card(drawn, "NAME", "2000000001"));

        Assert.That(card!.ArabicName, Is.EqualTo(expected));
    }

    [Test]
    public void IgnoresTheWatermark()
    {
        var words = Card("محمد", "MOHAMMAD", "2000000001");
        // Its middle sits just above the national number, so on the same line it would be read first.
        words.Add(Watermark("9861003819", 405, 391));

        Assert.That(IdentityCardReader.Read(words)!.NationalNumber, Is.EqualTo("2000000001"));
    }

    [Test]
    public void ReturnsNullForAnotherDocument()
    {
        var words = ArabicLine("شهادة عدم محكومية", 700, 450);
        words.AddRange(LatinLine("2000000001", 400, 400));

        Assert.That(IdentityCardReader.Read(words), Is.Null);
    }

    private static List<Word> Card(string arabicName, string englishName, string nationalNumber)
    {
        var words = ArabicLine("بطاقة شخصية رقمية", 480, 450);
        words.AddRange(ArabicLine(arabicName, 700, 420));
        words.AddRange(LatinLine(englishName, 140, 420));
        words.AddRange(ArabicLine("الرقم الوطني", 700, 400));
        words.AddRange(LatinLine(nationalNumber, 410, 401.8));
        words.AddRange(ArabicLine("الجنس", 700, 380));
        words.AddRange(ArabicLine("ذكر", 480, 380));
        words.AddRange(ArabicLine("فصيلة الدم", 700, 360));
        words.AddRange(LatinLine("O+", 460, 360));
        words.AddRange(ArabicLine("تاريخ المي#$د", 700, 340));
        words.AddRange(LatinLine("16/07/2000", 420, 340));
        words.AddRange(ArabicLine("اسم ا#6م", 700, 320));
        words.AddRange(ArabicLine("سلمى", 480, 320));
        words.AddRange(ArabicLine("رقم القيد", 700, 300));
        words.AddRange(LatinLine("013/049", 430, 300));
        words.AddRange(ArabicLine("تاريخ ا#8نتهاء", 700, 280));
        words.AddRange(LatinLine("20/07/2032", 420, 280));
        words.AddRange(ArabicLine("مكان ا#8قامة", 700, 260));
        words.AddRange(ArabicLine("ح#$وه/قصبه عجلون", 500, 260));
        words.AddRange(ArabicLine("رقم الهوية", 700, 240));
        words.AddRange(LatinLine("ABC12345", 425, 240));

        return words;
    }

    /// <summary>
    /// Lays the words out right to left, each word's letters left to right as drawn.
    /// </summary>
    private static List<Word> ArabicLine(string text, double right, double baseline)
    {
        var words = new List<Word>();
        foreach (var word in text.Split(' '))
        {
            var left = right - (word.Length * LetterWidth);
            words.Add(Word(word.Reverse(), left, baseline));
            right = left - LetterWidth;
        }

        return words;
    }

    private static List<Word> LatinLine(string text, double left, double baseline)
    {
        var words = new List<Word>();
        foreach (var word in text.Split(' '))
        {
            words.Add(Word(word, left, baseline));
            left += (word.Length + 1) * LetterWidth;
        }

        return words;
    }

    private static Word Word(IEnumerable<char> characters, double left, double baseline) =>
        new(characters.Select((character, index) => Letter(character, left + (index * LetterWidth), baseline)).ToList());

    private static Letter Letter(char character, double x, double baseline)
    {
        var (left, bottom, top) = UnmappedGlyphs.GetValueOrDefault(character, (0.05, 0, 0.56));
        var box = new PdfRectangle(x + (left * FontSize), baseline + (bottom * FontSize), x + LetterWidth, baseline + (top * FontSize));

        return Letter(character, new PdfPoint(x, baseline), new PdfPoint(x + LetterWidth, baseline), box);
    }

    /// <summary>
    /// The watermark runs at 30 degrees across the card.
    /// </summary>
    private static Word Watermark(string text, double left, double baseline) =>
        new(text.Select((character, index) =>
        {
            var start = new PdfPoint(left + (index * LetterWidth * 0.866), baseline + (index * LetterWidth * 0.5));
            var end = new PdfPoint(start.X + (LetterWidth * 0.866), start.Y + (LetterWidth * 0.5));

            return Letter(character, start, end, new PdfRectangle(start.X, start.Y, end.X, end.Y + (FontSize / 2)));
        }).ToList());

    private static Letter Letter(char character, PdfPoint start, PdfPoint end, PdfRectangle box) =>
        new(character.ToString(), box, start, end, LetterWidth, 1, Font, TextRenderingMode.Fill, GrayColor.Black, GrayColor.Black, FontSize, 0);
}
