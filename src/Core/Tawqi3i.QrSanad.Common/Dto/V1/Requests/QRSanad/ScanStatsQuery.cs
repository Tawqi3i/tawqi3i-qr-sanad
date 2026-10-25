namespace Tawqi3i.QrSanad.Common.Dto.V1.Requests.QRSanad;

using System.ComponentModel.DataAnnotations;

public class ScanStatsQuery
{
    /// <summary>
    /// Inclusive, UTC. Defaults to the start of the viewer's day 29 days before <see cref="To"/>.
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Exclusive, UTC. Defaults to now.
    /// </summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// The viewer's offset from UTC, so daily counts follow their calendar days
    /// (in a browser: <c>-new Date().getTimezoneOffset()</c>; Jordan is 180).
    /// </summary>
    [Range(-720, 840, ErrorMessage = "The {0} field must be between {1} and {2}")]
    public int UtcOffsetMinutes { get; set; }
}
