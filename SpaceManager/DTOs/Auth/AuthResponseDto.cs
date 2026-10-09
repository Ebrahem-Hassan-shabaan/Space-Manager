namespace SpaceManager.DTOs.Auth;

public record AuthResponseDto(bool Success, string Message, string Token="");

