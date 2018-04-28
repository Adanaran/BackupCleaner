using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BackupCleaner
{
  internal class Program
  {
    private const string DateRegex = @"\d{4}_\d{2}_\d{2}";

    public static void Main(string[] args)
    {
      var backupFolderPath = args.FirstOrDefault();
      if (!Directory.Exists(backupFolderPath))
      {
        Console.WriteLine($"Cannot find directory {backupFolderPath}");
        return;
      }

      var files = Directory.EnumerateFiles(backupFolderPath);

      var filesWithDates = new List<(string FilePath, DateTime Date)>();
      filesWithDates.AddRange(files.Select(file =>
      {
        try
        {
          var dateTime = DateTime.ParseExact(Regex.Match(file, DateRegex).Value, "yyyy_MM_dd", CultureInfo.InvariantCulture);
          return (file, dateTime);
        }
        catch (Exception exception)
        {
          Console.WriteLine($"file: {file}, exception: {exception.StackTrace}");
          return (file, DateTime.MinValue);
        }
      }));

      var filesByDate = filesWithDates.GroupBy(x => x.Date).ToDictionary(x => x.Key, x => x.Select(y => y.FilePath));

      var onlySundayBackupRetainmentThreshold = DateTime.Today.AddDays(-7);
      var maxBackupRetaimentThreshold = DateTime.Today.AddDays(-90);
      foreach (var (date, filePaths) in filesByDate)
      {
        var dateToKeep = date.DayOfWeek == DayOfWeek.Sunday ? maxBackupRetaimentThreshold : onlySundayBackupRetainmentThreshold;

        if (date < dateToKeep)
        {
          foreach (var filePath in filePaths)
          {
            if (File.Exists(filePath))
            {
              File.Delete(filePath);
            }
          }
        }
      }
    }
  }
}