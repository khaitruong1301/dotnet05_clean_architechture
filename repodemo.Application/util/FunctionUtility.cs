
public class FunctionUtility
{
    //slug helper
    public static string GenerateSlug(string input) //input: nguyễn văn a -> output: nguyen-van-a
    {
        // Convert to lowercase
        string slug = input.ToLower();

        // Replace spaces with hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

        // Remove invalid characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Trim hyphens from the ends
        slug = slug.Trim('-');

        return slug;
    }
}