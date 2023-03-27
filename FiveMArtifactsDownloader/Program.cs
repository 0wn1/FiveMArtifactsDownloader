using Newtonsoft.Json;
using System.IO.Compression;

class FiveMArtifactsDownloader
{
    static readonly string artifactsDir = "artifacts";
    static ChangelogResponse? changelogResponse;
    static string apiURL = "";
    static string? zípExtension;
    static string? input;

    static async Task Main(string[] args)

    {
        Console.Title = "FiveMArtifactsDownloader v1.1";
        Console.Title = "FiveMArtifactsDownloader v1.1.1";

        Console.WriteLine("Select the platform you want to use:");
        Console.WriteLine("1 - Windows");
        Console.WriteLine("2 - Linux");
        int platform;
        input = Console.ReadLine();

        while (!int.TryParse(input, out platform) || platform < 1 || platform > 2)
        {
            Console.WriteLine("Select the platform you want to use:");
            Console.WriteLine("1 - Windows");
            Console.WriteLine("2 - Linux");
            input = Console.ReadLine();
        }

        if (platform == 1)
        {
            apiURL = "https://changelogs-live.fivem.net/api/changelog/versions/win32/server";
            zípExtension = ".zip";
        }
        else if (platform == 2)
        {
            apiURL = "https://changelogs-live.fivem.net/api/changelog/versions/linux/server";
            zípExtension = ".tar.xz";
        }

        try
        {

            var client = new HttpClient();
            dynamic? versions = null;

            while (versions == null)
            {
                var response = await client.GetAsync(apiURL);
                var content = await response.Content.ReadAsStringAsync();
                versions = JsonConvert.DeserializeObject<dynamic>(content);
            }

            Console.WriteLine("Select a FiveM Artifact:");
            Console.WriteLine($"[1] - Latest | {versions.latest}");
            Console.WriteLine($"[2] - Recommended | {versions.recommended}");
            Console.WriteLine($"[3] - Optional | {versions.optional}");
            Console.WriteLine($"[4] - Critical | {versions.critical}");
            int choice = GetIntInput(1, 4);
            string[] artifactChoices = { "latest", "recommended", "optional", "critical" };
            string artifact = artifactChoices[choice - 1];
            string userInputChoice = $"{artifact}_download";

            if (Directory.Exists(artifactsDir))
            {
                Console.Clear();
                Console.WriteLine("Select what to do with the existing folder:");
                Console.WriteLine("[1] - Replace all files");
                Console.WriteLine("[2] - Rename current folder");
                int backupChoice = GetIntInput(1, 2);
                if (backupChoice == 2)
                {
                    BackupArtifactsFolder();
                }
                else
                {
                    Directory.Delete(artifactsDir, true);
                }
            }

            Directory.CreateDirectory(artifactsDir);
            await DownloadArtifact(userInputChoice, changelogResponse);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    static int GetIntInput(int min, int max)
    {
        while (true)
        {
            Console.Write($"Enter a number between {min} and {max}: ");
            string? inputString = Console.ReadLine();
            if (int.TryParse(inputString, out int input) && input >= min && input <= max)
            {
                return input;
            }
            Console.WriteLine("Invalid input. Please try again.");
        }
    }

    class ChangelogResponse
    {
        public string? latest_download { get; set; }
        public string? optional_download { get; set; }
        public string? recommended_download { get; set; }
        public string? critical_download { get; set; }
    }

    static async Task DownloadArtifact(string userInputChoice, ChangelogResponse? changelogResponse)
    {
        using (HttpClient client = new())
        {
            using HttpResponseMessage response = await client.GetAsync(apiURL);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            changelogResponse = System.Text.Json.JsonSerializer.Deserialize<ChangelogResponse>(responseString);
        }

        string? downloadURL = changelogResponse?.GetType().GetProperty(userInputChoice)?.GetValue(changelogResponse)?.ToString();

        using (HttpClient client = new())
        {
            Console.Clear();
            Console.WriteLine($"Downloading " + $"{userInputChoice + zípExtension}...");
            using Stream stream = await client.GetStreamAsync(downloadURL);
            using FileStream fs = new($"{artifactsDir}/{userInputChoice + zípExtension}", FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fs);
        }
        if (input == "1")
        {
            Console.WriteLine($"Extracting " + $"{userInputChoice + zípExtension}...");
            ZipFile.ExtractToDirectory($"{artifactsDir}/{userInputChoice + zípExtension}", artifactsDir);

            Console.WriteLine($"Deleting " + $"{userInputChoice + zípExtension}...");
            File.Delete($"{artifactsDir}/{userInputChoice + zípExtension}");
        }
        Console.WriteLine($"Download successfully completed!");
    }

    static void BackupArtifactsFolder()
    {
        string backupDirName = $"backup_{DateTime.Now:yyyyMMddHHmmss}";
        Directory.Move(artifactsDir, backupDirName);
    }
}