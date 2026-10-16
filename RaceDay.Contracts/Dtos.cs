namespace RaceDay.Contracts
{
    // Authentication DTOs
    public record LoginRequest(string Email, string Password);

    public record RegisterRequest(
        string FirstName,
        string LastName,
        string Email,
        string Password,
        string Role
    );

    public record AuthResponse(
        string Token,
        string UserId,
        string Email,
        string FirstName,
        string LastName,
        string Role
    );

    // Profile DTOs
    public record ProfileDto(
        string UserId,
        string FirstName,
        string LastName,
        string Email,
        string Role
    );

    public record UpdateProfileRequest(
        string FirstName,
        string LastName,
        string Email
    );

    // Event DTOs
    public record CreateEventRequest(
        string Title,
        string Description,
        DateTime Date,
        string Location
    );

    public record UpdateEventRequest(
        string Title,
        string Description,
        DateTime Date,
        string Location
    );

    public record EventDto(
        int Id,
        string Title,
        string Description,
        DateTime Date,
        string Location,
        string CreatedByUserId
    );

    // Category DTOs
    public record CreateCategoryRequest(
        int EventId,
        string Name,
        decimal DistanceKm
    );

    public record CategoryDto(
        int Id,
        int EventId,
        string Name,
        decimal DistanceKm
    );

    // Enrolment DTOs
    public record CreateEnrolmentRequest(
        int CategoryId
    );

    public record UpdateEnrolmentStatusRequest(
        string Status
    );

    public record EnrolmentDto(
        int Id,
        string UserId,
        int CategoryId,
        string Status,
        DateTime EnrolledAt
    );

    // Result DTOs
    public record CreateResultRequest(
        int EnrolmentId,
        TimeSpan CompletionTime,
        int Rank
    );

    public record ResultDto(
        int Id,
        int EnrolmentId,
        TimeSpan CompletionTime,
        int Rank
    );

    // File Upload Response (Part 3)
    public record UploadResponse(
        string Url,
        string FileName
    );
}
