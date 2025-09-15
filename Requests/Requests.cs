using System.ComponentModel.DataAnnotations;

namespace Datapac.Requests
{
    public class BookCreationRequest
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be at least 1 character.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Author must be at least 1 character.")]
        public string Author { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "TotalCopies must be at least 0")]
        public int TotalCopies { get; set; }
    }
    public class BookUpdateRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Title must be at least 1 character.")]
        public string? Title { get; set; } = null!;

        [StringLength(100, MinimumLength = 1, ErrorMessage = "Author must be at least 1 character.")]
        public string? Author { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "TotalCopies must be at least 0")]
        public int? TotalCopies { get; set; }
    }

    // public record LoanCreationRequest(
    //     int BookId,
    //     int UserId,
    //     DateOnly? ExpirationDate
    //     );
    
    public class LoanCreationRequest : IValidatableObject
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateOnly? ExpirationDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExpirationDate.HasValue)
            {
                var minDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1));

                if (ExpirationDate.Value < minDate)
                {
                    yield return new ValidationResult(
                        "ExpirationDate must be tomorrow or later",
                        new[] { nameof(ExpirationDate) });
                }
            }
        }
    }
}