using Microsoft.Extensions.Configuration;
using Octokit;

var repos = new List<(string, string)>
{
    ("dotnet","maui"),
    ("xamarin","AndroidX"),
    ("jsuarezruiz","AlohaKit"),
    ("xamarin","XamarinCommunityToolkit"),
    ("xamarin","FacebookComponents"),
    ("XAM-Consulting","FreshMvvm.Maui"),
    ("xamarin","GoogleAPIsForiOSComponents"),
    ("xamarin","GooglePlayServicesComponents"),
    ("jsuarezruiz","MauiAnimation"),
    ("PrismLibrary","Prism.Maui"),
    ("smstuebe","xamarin-fingerprint"),
    ("jamesmontemagno","InAppBillingPlugin"),
    ("jamesmontemagno","StoreReviewPlugin"),
    ("luismts","ValidationRulesPlugin"),
    ("reactiveui","reactiveui"),
    ("shinyorg","shiny"),
    ("mono","SkiaSharp"),
    ("jsuarezruiz","TemplateUI"),
    ("aritchie","userdialogs")
};

var config = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    .AddJsonFile("appsettings.json", true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables()
    .Build();

string token = config["GitHubToken"];
if (token == null)
{
    Console.WriteLine("GitHubToken config setting missing. Configure a token at https://github.com/settings/tokens and add it to user secrets.");
    Environment.Exit(1);
}

//https://docs.github.com/en/enterprise-cloud@latest/authentication/authenticating-with-saml-single-sign-on/authorizing-a-personal-access-token-for-use-with-saml-single-sign-on
var creds = new Credentials(token);
var github = new GitHubClient(new ProductHeaderValue("GitHubAvatarDownloader")) { Credentials = creds };

//var github = new GitHubClient(new ProductHeaderValue("GitHubAvatarDownloader"));

//Using a hashset to only show an avatar once
var allContributors = new HashSet<RepositoryContributor>();

foreach (var repo in repos)
{
    var contributors = await github.Repository.GetAllContributors(repo.Item1, repo.Item2);

    foreach (var contributor in contributors)
    {
        allContributors.Add(contributor);
    }
}

//Create directory if it doesn't exist
string directory = config["directory"] ?? "download";
Directory.CreateDirectory(directory);

int avatarCount = 0;

var httpClient = new HttpClient();
foreach (var c in allContributors)
{
    var imageBytes = await httpClient.GetByteArrayAsync(c.AvatarUrl);

    //Anything < 1.5KB is a gravatar or single color graphic
    if (imageBytes.Length > 1500)
    {
        await File.WriteAllBytesAsync($"{directory}/{c.Login}.png", imageBytes);
        avatarCount++;
    }
}

int h = (int)Math.Round(Math.Sqrt(avatarCount / 1.7778));
int w = (int)(h * 1.7778);

Console.WriteLine("Use the following to generate a mosaic:");
Console.WriteLine($"magick montage -geometry 200x200+0+0 -tile {w}x{h} {directory}/*.png output.png");
Console.WriteLine("Press any key to quit.");
Console.ReadLine();