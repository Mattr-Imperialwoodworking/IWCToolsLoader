namespace IWCToolsLoader.Core;

public sealed class FileCopyService
{
    private readonly Logger _logger;

    public FileCopyService(Logger logger)
    {
        _logger = logger;
    }

    public void CopyFolder(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(sourceDir))
            throw new DirectoryNotFoundException($"Source folder not found: {sourceDir}");

        if (Directory.Exists(destinationDir))
        {
            // Copying into a clean version folder avoids stale files from older releases.
            Directory.Delete(destinationDir, recursive: true);
        }

        Directory.CreateDirectory(destinationDir);

        foreach (string dirPath in Directory.GetDirectories(sourceDir, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(sourceDir, dirPath);
            Directory.CreateDirectory(Path.Combine(destinationDir, rel));
        }

        foreach (string sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
        {
            string rel = Path.GetRelativePath(sourceDir, sourceFile);
            string destFile = Path.Combine(destinationDir, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
            File.Copy(sourceFile, destFile, overwrite: true);
        }

        _logger.Info($"Copied folder {sourceDir} -> {destinationDir}");
    }

    public void CopyFile(string sourceFile, string destinationFile)
    {
        if (!File.Exists(sourceFile))
            throw new FileNotFoundException($"Source file not found: {sourceFile}", sourceFile);

        Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
        File.Copy(sourceFile, destinationFile, overwrite: true);
        _logger.Info($"Copied {sourceFile} -> {destinationFile}");
    }
}
