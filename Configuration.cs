namespace Blog;

public static class Configuration
{
    //token - jwt - Json Web Token
    public static string JwtKey = "ZmVkYWY3ZDg4NjNiNDhlMTk3YjkyODdkNDkyYjcwOGU=";

    //Uma segunda forma que pode ser usada no lugar do token jwt, é o Api key
    public static string ApiKeyName = "api_key";
    public static string ApiKey = "curso_api_123456";

    public static SmtpConfiguration Stmp = new();

    public class SmtpConfiguration
    {
        public string Host {get; set;}
        public int Port {get; set;} = 25;
        public string Username {get; set;}
        public string Password {get; set;}
    
    }
}