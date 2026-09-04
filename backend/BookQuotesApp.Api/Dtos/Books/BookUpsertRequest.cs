using System.ComponentModel.DataAnnotations;

namespace BookQuotesApp.Api.Dtos.Books;

public record BookUpsertRequest(
    [Required(ErrorMessage = "Titel krävs.")]
    [MaxLength(200, ErrorMessage = "Titeln får vara högst 200 tecken.")]
    string Title,

    [Required(ErrorMessage = "Författare krävs.")]
    [MaxLength(150, ErrorMessage = "Författarens namn får vara högst 150 tecken.")]
    string Author,

    [Required(ErrorMessage = "Utgivningsdatum krävs.")]
    DateOnly PublishedDate
);
