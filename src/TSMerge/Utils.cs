namespace TSMerge;

internal static class Utils
{
    public static void MergeMultipleFiles(string[] inputFiles, string outputFile, double patternSizeInMb)
    {
        if (inputFiles.Length < 2)
        {
            throw new ArgumentException("At least two files are required for merging.");
        }

        if (string.IsNullOrEmpty(outputFile))
        {
            throw new ArgumentException("One output file must be specified.");
        }

        using var fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.ReadWrite);

        for (var i = 0; i < inputFiles.Length; i++)
        {
            var currentFile = Path.GetFullPath(inputFiles[i]);
            Console.WriteLine($"[{i + 1}/{inputFiles.Length}]: Processing {Path.GetFileName(currentFile)}");
            if (i == 0)
            {
                Console.WriteLine($"\tFirst input. Copying entire file...");
                // For the first file, just copy its content to the output file
                using var fsCurrent = new FileStream(currentFile, FileMode.Open, FileAccess.Read);
                fsCurrent.CopyTo(fsOutput);
            }
            else
            {
                // For subsequent files, merge them with the previous content
                MergeFilesWithSearch(currentFile, fsOutput, patternSizeInMb);
            }
        }

        Console.WriteLine($"Files merged successfully into {Path.GetFullPath(outputFile)}.");
    }

    static void MergeFilesWithSearch(string fileB, FileStream fsOutput, double patternSizeInMb)
    {
        int patternSize = (int)(patternSizeInMb * 1024 * 1024); // n MB

        // Step 1: Read the first n MB of B.ts
        using var fsB = new FileStream(fileB, FileMode.Open, FileAccess.Read);
        var readSize = (int)Math.Min(patternSize, fsB.Length);
        var searchPattern = new byte[readSize];
        _ = fsB.Read(searchPattern, 0, readSize);
        fsB.Position = 0; // Go to file head

        // Step 2: Search in the current content of fsOutput from the end to the beginning
        long matchPosition = FindPatternFromEnd(fsOutput, searchPattern);

        // Step 3: Merge fsOutput and B.ts into fsOutput
        if (matchPosition >= 0)
        {
            Console.WriteLine($"\tMatch found at position {matchPosition}. Copying file...");

            // Move fsOutput position to the match point
            fsOutput.Seek(matchPosition, SeekOrigin.Begin);

            // Write the entirety of B.ts
            fsB.CopyTo(fsOutput);
        }
        else
        {
            Console.WriteLine("\tNo match found. Copying entire file...");

            // Write entire B.ts
            fsB.Position = 0;
            fsB.CopyTo(fsOutput);
        }
    }

    static long FindPatternFromEnd(FileStream fs, byte[] pattern)
    {
        const int bufferSize = 4 * 1024 * 1024; // 4 MB buffer
        var buffer = new byte[bufferSize + pattern.Length - 1];

        long fileLength = fs.Length;

        for (var position = fileLength; position > 0; position -= bufferSize)
        {
            // Ensure read at least pattern.Length bytes for overlap
            var bytesToRead = (int)Math.Min(bufferSize, position) + pattern.Length - 1;
            var overlapStart = Math.Max(0, bufferSize - pattern.Length + 1);

            fs.Position = Math.Max(0, position - bytesToRead);
            _ = fs.Read(buffer, 0, bytesToRead);

            // Check for the pattern in this buffer
            var matchIndex = FindPatternInBuffer(buffer, bytesToRead, pattern);
            if (matchIndex >= 0)
            {
                return position - bytesToRead + matchIndex;
            }

            // Shift overlap portion into the beginning of the buffer for next read
            Array.Copy(buffer, overlapStart, buffer, 0, pattern.Length - 1);
        }

        return -1; // No match found
    }

    // Boyer-Moore like
    static int FindPatternInBuffer(byte[] buffer, int validLength, byte[] pattern)
    {
        for (var i = validLength - pattern.Length; i >= 0; i--)
        {
            var match = true;
            for (var j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }
}