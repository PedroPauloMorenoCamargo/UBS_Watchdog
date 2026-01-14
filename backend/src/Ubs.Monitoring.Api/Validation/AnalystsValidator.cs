using System.Buffers;
using FluentValidation;
using Ubs.Monitoring.Api.Contracts;

namespace Ubs.Monitoring.Api.Validation.Analysts;

public sealed class UpdateProfilePictureRequestValidator : AbstractValidator<UpdateProfilePictureRequest>
{
    private const int MaxImageBytes = 2 * 1024 * 1024; // 2 MiB

    private const int MaxBase64Chars = ((MaxImageBytes + 2) / 3) * 4 + 128;

    public UpdateProfilePictureRequestValidator()
    {
        RuleFor(x => x.ProfilePictureBase64)
        .Must(BeNullOrValidImageBase64).WithMessage("ProfilePictureBase64 must be a valid base64 image (raw base64 or data URI).")
        .DependentRules(() =>
        {
            RuleFor(x => x.ProfilePictureBase64)
                .Must(v => BeNullOrWithinMaxBytes(v, MaxImageBytes)).WithMessage($"Profile picture must be at most {MaxImageBytes} bytes after decoding.");
        });

    }

    private static bool BeNullOrValidImageBase64(string? value)
    {
        var payload = ExtractBase64Payload(value, out var invalid);
        if (invalid) return false;
        if (payload is null) return true;
        return payload.Length > 0 && (payload.Length % 4 == 0);
    }

    private static bool BeNullOrWithinMaxBytes(string? value, int maxBytes)
    {
        var payload = ExtractBase64Payload(value, out var invalid);
        if (invalid) return false;
        if (payload is null) return true;

        var maxDecoded = (payload.Length / 4) * 3 + 3;

        var buffer = ArrayPool<byte>.Shared.Rent(Math.Min(maxDecoded, maxBytes + 16));
        try
        {
            if (!Convert.TryFromBase64String(payload, buffer, out var bytesWritten))
                return false;

            return bytesWritten <= maxBytes;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private static string? ExtractBase64Payload(string? value, out bool invalid)
    {
        invalid = false;

        if (string.IsNullOrWhiteSpace(value))
            return null;

        var s = value.Trim();


        if (s.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            const string marker = ";base64,";
            var idx = s.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
            {
                invalid = true;
                return null;
            }

            var mime = s.Substring(5, idx - 5);
            if (!mime.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                invalid = true;
                return null;
            }

            return s[(idx + marker.Length)..].Trim();
        }

        return s;
    }
}
