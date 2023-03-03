using System.IO.Compression;
using Newtonsoft.Json;

class FiveMArtifactsDownloader
{
    static readonly string apiURL = "https://changelogs-live.fivem.net/api/changelog/versions/win32/server";
    static readonly string artifactsDir = "artifacts";
    static ChangelogResponse? changelogResponse;

    static async Task Main(string[] args)

    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        Console.Title = "FiveMArtifactsDownloader";
        try
        {

            var client = new HttpClient();
            var response = await client.GetAsync(apiURL);
            var content = await response.Content.ReadAsStringAsync();
            var versions = JsonConvert.DeserializeObject<dynamic>(content);

            if (versions == null) {
                return;
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
        public string? latest { get; set; }
        public string? latest_download { get; set; }
        public string? optional { get; set; }
        public string? optional_download { get; set; }
        public string? recommended { get; set; }
        public string? recommended_download { get; set; }
        public string? critical { get; set; }
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
            Console.WriteLine($"Downloading selected build...");
            using Stream stream = await client.GetStreamAsync(downloadURL);
            using FileStream fs = new($"{artifactsDir}/{userInputChoice}.zip", FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fs);
        }

        Console.WriteLine($"Extracting selected build...");
        ZipFile.ExtractToDirectory($"{artifactsDir}/{userInputChoice}.zip", artifactsDir);

        Console.WriteLine($"Deleting downloaded zip file...");
        File.Delete($"{artifactsDir}/{userInputChoice}.zip");
        Console.WriteLine($"Download successfully completed!");
    }

    static void BackupArtifactsFolder()
    {
        string backupDirName = $"backup_{DateTime.Now:yyyyMMddHHmmss}";
        Directory.Move(artifactsDir, backupDirName);
        //Console.WriteLine($"Artifacts folder was renamed to \"{backupDirName}\"");
    }
}