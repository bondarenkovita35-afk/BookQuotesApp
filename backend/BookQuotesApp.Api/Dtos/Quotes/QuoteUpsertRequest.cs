using System.ComponentModel.DataAnnotations;

namespace BookQuotesApp.Api.Dtos.Quotes;

public record QuoteUpsertRequest(
    [Required(ErrorMessage = "Citattext krävs.")]
    [MaxLength(1000, ErrorMessage = "Citatet får vara högst 1000 tecken.")]
    string Text,

    [MaxLength(150, ErrorMessage = "Författarens namn får vara högst 150 tecken.")]
    string? Author
);
