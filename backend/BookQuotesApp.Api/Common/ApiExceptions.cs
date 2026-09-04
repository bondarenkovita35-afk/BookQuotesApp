namespace BookQuotesApp.Api.Common;

/// <summary>
/// Kastas när en resurs inte hittas, eller när den tillhör en annan användare.
/// Samma undantag används i båda fallen med avsikt, så att en anropare inte
/// kan avgöra om ett id existerar men tillhör någon annan.
/// </summary>
public class NotFoundException(string message) : Exception(message);

public class ConflictException(string message) : Exception(message);
