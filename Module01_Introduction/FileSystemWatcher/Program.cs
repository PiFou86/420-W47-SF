// Détermination des chemins d'accès
// pour les dossiers de traitement, d'erreur et d'archivage
string basePath = AppDomain.CurrentDomain.BaseDirectory;
string pathToImport = Path.Combine(basePath, "ToImport");
string pathProcessing = Path.Combine(basePath, "Processing");
string pathError = Path.Combine(basePath, "Error");
string pathProcessed = Path.Combine(basePath, "Processed");

// Création des dossiers s'ils n'existent pas
if (!Directory.Exists(pathToImport)) Directory.CreateDirectory(pathToImport);
if (!Directory.Exists(pathProcessing)) Directory.CreateDirectory(pathProcessing);
if (!Directory.Exists(pathError)) Directory.CreateDirectory(pathError);
if (!Directory.Exists(pathProcessed)) Directory.CreateDirectory(pathProcessed);

// Mise en place de la surveillance des fichiers
Console.Out.WriteLine("Starting to watch for changes...");
FileSystemWatcher watcher = new FileSystemWatcher
{
    Path = pathToImport,
    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
    Filter = "*.*"
};
// En cas de changement de fichier, appeler la méthode OnChanged
watcher.Changed += OnChanged;

// Traiter les fichiers déjà présents dans le dossier
foreach (string file in Directory.GetFiles(watcher.Path))
{
    ProcessFile(file);
}

// Commencer à surveiller les fichiers
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

// Traiter le fichier
void ProcessFile(string fullFilePath)
{
    // Vérifier si le fichier existe : plusieurs événements peuvent être déclenchés pour le même fichier
    if (!File.Exists(fullFilePath)) return;

    string fileName = Path.GetFileName(fullFilePath);

    string archiveFileName = Guid.NewGuid().ToString();
    if (Path.HasExtension(fileName))
    {
        archiveFileName += "." + Path.GetExtension(fileName);
    }

    // Déplacement du fichier dans le dossier de traitement
    string pathErrorFileName = Path.Combine(pathError, archiveFileName);
    string processingFileNewPath = Path.Combine(pathProcessing, archiveFileName);
    try
    {
        Console.Out.WriteLine($"Processing file: {fullFilePath}");
        Console.Out.WriteLine($"Moving file to processing folder");
        File.Move(fullFilePath, processingFileNewPath);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error processing file: {ex.Message}");
        File.Move(fullFilePath, pathErrorFileName);
        Console.Error.WriteLine($"File moved to error folder: {pathErrorFileName}");
    }

    string processedFileName = Path.Combine(pathProcessed, archiveFileName);
    try
    {
        Console.Out.WriteLine($"Processing file: {processingFileNewPath}");
        Console.Out.WriteLine($"Simulating processing of file: {processingFileNewPath} (2s)");
        Thread.Sleep(2000); // Votre algorithme ici à la place de Thread.Sleep

        // Simulation d'erreur avec un taux de 10%
        Random random = new Random();
        if (random.Next(1, 11) == 1)
        {
            throw new Exception("Simulated error during processing");
        }

        Console.Out.WriteLine($"File processed");
        Console.Out.WriteLine($"Moving file to archive folder");
        File.Move(processingFileNewPath, processedFileName);
        Console.Out.WriteLine($"File moved to archive folder: {pathProcessed}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error processing file: {ex.Message}");
        File.Move(processingFileNewPath, pathErrorFileName);
        Console.Error.WriteLine($"File moved to error folder: {pathError}");
    }
}