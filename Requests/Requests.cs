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

    public record LoanCreationRequest(
        int BookId,
        int UserId);
}