namespace PortfolioApi.Models;

public class Certificate
{
    public int Id { get; set; }
    public int User_Id { get; set; }
    public string Title { get; set; } = "";
    public string Image_Url { get; set; } = "";
}
