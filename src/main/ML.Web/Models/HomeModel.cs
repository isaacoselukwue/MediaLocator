namespace ML.Web.Models;

public class DailyMediaDto
{
    public string? WordOfTheDay { get; set; }
    public List<AudioSearchResult> AudioSearchResults { get; set; } = [];
    public List<ImageSearchResult> ImageSearchResults { get; set; } = [];
}