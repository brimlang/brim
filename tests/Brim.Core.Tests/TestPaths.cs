namespace Brim.Core.Tests;

internal static class TestPaths
{
  static string? _tmpTestsDirectory;

  static string LocateTmpDirectory()
  {
    string? current = AppContext.BaseDirectory;
    while (!string.IsNullOrEmpty(current))
    {
      string candidate = Path.Combine(current, "tmp");
      if (Directory.Exists(candidate))
        return candidate;

      DirectoryInfo? parent = Directory.GetParent(current);
      if (parent is null)
        break;
      current = parent.FullName;
    }

    throw new InvalidOperationException("Unable to locate repository tmp directory.");
  }

  static string EnsureTestsTmpDirectory()
  {
    if (_tmpTestsDirectory is not null)
      return _tmpTestsDirectory;

    string tmpRoot = LocateTmpDirectory();
    string testsTmp = Path.Combine(tmpRoot, "Brim.Core.TestsTmp");
    Directory.CreateDirectory(testsTmp);
    _tmpTestsDirectory = testsTmp;
    return testsTmp;
  }

  public static string CreateTmpFile(string fileNamePrefix, string contents, string extension = ".txt")
  {
    string directory = EnsureTestsTmpDirectory();
    string fileName = $"{fileNamePrefix}_{Guid.NewGuid():N}{extension}";
    string path = Path.Combine(directory, fileName);
    File.WriteAllText(path, contents);
    return path;
  }

  public static void DeleteIfExists(string path)
  {
    if (File.Exists(path))
      File.Delete(path);
  }
}
