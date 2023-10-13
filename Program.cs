using ImageMagick;
using Spectre.Console;
using Microsoft.Extensions.Configuration;
using Octokit;

// Repos sourced from https://github.com/dotnet/core/blob/main/Documentation/core-repos.md
var repos = new List<(string, string)>
{
    ("dotnet","core"),
    ("dotnet","runtime"),
    ("dotnet","docs"),
    ("dotnet","dotnet-api-docs"),
    ("dotnet","project-system"),
    ("dotnet","sdk"),
    ("dotnet","installer"),
    ("dotnet","extensions"),
    ("dotnet","dotnet-docker"),
    ("dotnet","templating"),
    ("dotnet","test-templates"),
    ("dotnet","winforms"),
    ("dotnet","wpf"),
    ("dotnet","maui"),
    ("microsoft","dotnet-framework-docker"),
    ("dotnet","standard"),
    ("dotnet","aspnetcore"),
    ("dotnet","websdk"),
    ("dotnet","msbuild"),
    ("dotnet","efcore"),
    ("dotnet","sqlclient"),
    ("dotnet","machinelearning"),
    ("dotnet","machinelearning-modelbuilder"),
    ("dotnet","ml-api-docs"),
    ("dotnet","spark"),
    ("dotnet","roslyn"),
    ("dotnet","csharplang"),
    ("dotnet","vblang"),
    ("dotnet","fsharp"),
    ("fsharp","fslang-design"),
    ("fsharp","fslang-suggestions"),
    ("nuget","home"),
    ("nuget","nugetgallery"),
    ("dotnet","wcf"),
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
    AnsiConsole.WriteLine("GitHubToken config setting missing. Configure a token at https://github.com/settings/tokens and add it to user secrets.");
    Environment.Exit(1);
}

//https://docs.github.com/en/enterprise-cloud@latest/authentication/authenticating-with-saml-single-sign-on/authorizing-a-personal-access-token-for-use-with-saml-single-sign-on
var creds = new Credentials(token);
var github = new GitHubClient(new ProductHeaderValue("GitHubAvatarDownloader")) { Credentials = creds };

//var github = new GitHubClient(new ProductHeaderValue("GitHubAvatarDownloader"));

//Using a hashset to only show an avatar once
var allContributors = new HashSet<RepositoryContributor>();

await AnsiConsole.Status()
    .StartAsync("Getting list of contributors...", async ctx =>
    {
        foreach (var repo in repos)
        {
            AnsiConsole.MarkupLine($"Getting contributors from {repo.Item1}/{repo.Item2}");
            var contributors = await github.Repository.GetAllContributors(repo.Item1, repo.Item2);

            foreach (var contributor in contributors)
            {
                allContributors.Add(contributor);
            }
        }
    });


//Create directory if it doesn't exist
string directory = config["directory"] ?? "download";
Directory.CreateDirectory(directory);

int avatarCount = 0;

var httpClient = new HttpClient();
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var downloadTask = ctx.AddTask("Downloading avatars...", maxValue: allContributors.Count);
        foreach (var c in allContributors)
        {
            var imageBytes = await httpClient.GetByteArrayAsync(c.AvatarUrl);

            //Anything < 1.5KB is a gravatar or single color graphic
            if (imageBytes.Length > 1500)
            {
                // Only write if file doesn't exist, useful if re-running
                if (!File.Exists($"{directory}/{c.Login}.png"))
                {
                    await File.WriteAllBytesAsync($"{directory}/{c.Login}.png", imageBytes);
                }
                avatarCount++;
            }
            downloadTask.Increment(1);
        }
    });

using var images = new MagickImageCollection();

// get the files from the directory in random order
var files = Directory.GetFiles(directory).OrderBy(x => Random.Shared.Next());
foreach (var file in files)
{
    images.Add(file);
}

int actualAvatars = images.Count;
int h = (int)Math.Round(Math.Sqrt(actualAvatars / 1.7778));
int w = (int)Math.Round(h * 1.7778);

var settings = new MontageSettings
{
    Geometry = new MagickGeometry(200, 200),
    TileGeometry = new MagickGeometry($"{w}x{h}"),
    BorderWidth = 0,
    BackgroundColor = MagickColors.Transparent
};

int surplus = images.Count - (w * h);
if (surplus > 0)
{
    AnsiConsole.WriteLine($"Removing {surplus} random avatars to fit an even multiple.");
    for (int i = 0; i < surplus; i++)
    {
        // remove a random image from the middle
        int index = Random.Shared.Next(w * h - surplus);
        images.RemoveAt(index);
    }
}

AnsiConsole.Status()
    .Start("Generating image...", ctx =>
    {
        using var result = images.Montage(settings);
        result.Write("contributors.png");
    });

AnsiConsole.WriteLine($"Total avatars downloaded: {actualAvatars}");
AnsiConsole.WriteLine($"Image written to contributors.png");
AnsiConsole.WriteLine("Use the following to tweak and generate a mosaic:");
AnsiConsole.WriteLine($"magick montage -geometry 200x200+0+0 -tile {w}x{h} {directory}/*.png contributors.png");
AnsiConsole.WriteLine("Press any key to quit.");
Console.ReadLine();