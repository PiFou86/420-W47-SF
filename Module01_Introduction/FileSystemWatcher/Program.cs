string basePath = AppDomain.CurrentDomain.BaseDirectory;

if (!Directory.Exists(Path.Combine(basePath, "ToImport")))
{
    Directory.CreateDirectory(Path.Combine(basePath, "ToImport"));
}
if (!Directory.Exists(Path.Combine(basePath, "Archive")))
{
    Directory.CreateDirectory(Path.Combine(basePath, "Archive"));
}
if (!Directory.Exists(Path.Combine(basePath, "Error")))
{
    Directory.CreateDirectory(Path.Combine(basePath, "Error"));
}

Console.Out.WriteLine("Starting to watch for changes...");
var watcher = new FileSystemWatcher
{
    Path = Path.Combine(basePath, "ToImport"),
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
    Filter = "*.*"
};

watcher.Changed += OnChanged;

foreach (string file in Directory.GetFiles(watcher.Path))
{
    ProcessFile(file);
}
watcher.Created += OnChanged;

watcher.EnableRaisingEvents = true;
Console.Out.WriteLine("Press 'q' to quit the sample.");
while (Console.Read() != 'q')
{
    // Keep the application running until 'q' is pressed.
}

void OnChanged(object source, FileSystemEventArgs e)
{
    Console.Out.WriteLine($"Detected file : {e.FullPath} {e.ChangeType}");
    ProcessFile(e.FullPath);
}

void ProcessFile(string fullFilePath) {
    if (!File.Exists(fullFilePath)) return;

    string fileName = Path.GetFileName(fullFilePath);

    string archiveFileName = Guid.NewGuid().ToString();
    if (Path.HasExtension(fileName)) {
        archiveFileName += "." + Path.GetExtension(fileName);
    }

    string archivePath = Path.Combine(basePath, "Archive", archiveFileName);
    string errorPath = Path.Combine(basePath, "Error", archiveFileName);
    try
    {
        Console.Out.WriteLine($"Processing file: {fullFilePath}");
        Thread.Sleep(2000); // Votre algorithme
        Console.Out.WriteLine($"File processed");
        Console.Out.WriteLine($"Moving file to archive folder");
        File.Move(fullFilePath, archivePath);
        Console.Out.WriteLine($"File moved to archive folder: {archivePath}");
    }
    catch (Exception ex)
    {
        Console.Out.WriteLine($"Error processing file: {ex.Message}");
        // Move the file to the error folder
        File.Move(fullFilePath, errorPath);
        Console.Out.WriteLine($"File moved to error folder: {errorPath}");
    }
}