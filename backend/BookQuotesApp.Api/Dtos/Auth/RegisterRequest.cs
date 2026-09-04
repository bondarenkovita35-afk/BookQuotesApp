using System.ComponentModel.DataAnnotations;

namespace BookQuotesApp.Api.Dtos.Auth;

public record RegisterRequest(
    [Required(ErrorMessage = "E-postadress krävs.")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress.")]
    [MaxLength(256, ErrorMessage = "E-postadressen får vara högst 256 tecken.")]
    string Email,

    [Required(ErrorMessage = "Lösenord krävs.")]
    [MinLength(8, ErrorMessage = "Lösenordet måste vara minst 8 tecken.")]
    [MaxLength(100, ErrorMessage = "Lösenordet får vara högst 100 tecken.")]
    string Password
);
