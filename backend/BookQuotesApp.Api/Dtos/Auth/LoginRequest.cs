using System.ComponentModel.DataAnnotations;

namespace BookQuotesApp.Api.Dtos.Auth;

public record LoginRequest(
    [Required(ErrorMessage = "E-postadress krävs.")]
    [EmailAddress(ErrorMessage = "Ange en giltig e-postadress.")]
    string Email,

    [Required(ErrorMessage = "Lösenord krävs.")]
    string Password
);
