using System.ComponentModel.DataAnnotations;

namespace Datapac.Utils;

public static class ValidationUtil
{
    public static IDictionary<string, string[]>? Validate<T>(T model)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        var ok = Validator.TryValidateObject(model!, context, results, validateAllProperties: true);
        if (ok) return null;

        return results.ToDictionary(
         v => v.MemberNames.FirstOrDefault() ?? "Error",
         v => new string[] { v.ErrorMessage! });
    }
}