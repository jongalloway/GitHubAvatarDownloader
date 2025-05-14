# GitHubAvatarDownloader

This is a simple app to generate a mosaic of contributors to a list of GitHub repositories.

This is currently just hardcoded but could easily be abstracted to a JSON file if anyone other than me is using it.

```csharp
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
```

![dotnet-contributors-1920x1080](https://github.com/jongalloway/GitHubAvatarDownloader/assets/68539/08a4c1ac-57e2-4eab-9fe4-04c0cb731c04)
