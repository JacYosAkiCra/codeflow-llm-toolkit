using System.Text;
using System.Text.RegularExpressions;

namespace CodeTextToolkit.Services;

public class FileProcessingService
{
    // Supported code file extensions
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".c", ".cpp", ".cc", ".cxx", ".h", ".hpp",
        ".cs",
        ".py", ".pyw",
        ".js", ".jsx", ".ts", ".tsx",
        ".java",
        ".go",
        ".rs",
        ".php",
        ".rb",
        ".swift",
        ".kt", ".kts",
        ".html", ".htm", ".css", ".scss", ".sass",
        ".xml", ".json", ".yaml", ".yml",
        ".sql",
        ".sh", ".bash", ".zsh",
        ".bat", ".cmd",
        ".ps1", ".psm1", ".psd1",
        ".md", ".txt", ".log"
    };

    public bool IsSupportedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        return SupportedExtensions.Contains(ext);
    }

    public string GetFileKind(string fileName, string[] lines)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        
        return ext switch
        {
            ".ps1" => "powershell",
            ".py" or ".pyw" => "python",
            ".js" or ".jsx" => "js",
            ".ts" or ".tsx" => "js",
            ".json" => "json",
            ".jsonl" or ".ndjson" => "ndjson",
            ".cs" or ".java" or ".cpp" or ".c" or ".h" or ".go" => "cstyle",
            ".rb" => "ruby",
            ".md" => "markdown",
            ".log" => "log",
            ".yaml" or ".yml" => "yaml",
            ".xml" => "xml",
            ".html" or ".htm" => "html",
            _ => DetectFileKind(lines)
        };
    }

    private string DetectFileKind(string[] lines)
    {
        var sample = lines.Take(200).ToArray();
        var nonEmpty = sample.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        
        if (nonEmpty.Length == 0) return "text";

        // Check for NDJSON
        var ndjsonCount = nonEmpty.Count(l => Regex.IsMatch(l, @"^[\s\t]*[\{\[].*[\}\]]\s*,?\s*$"));
        if (ndjsonCount >= Math.Ceiling(nonEmpty.Length * 0.6)) return "ndjson";

        // Check for JSON
        var firstLine = nonEmpty[0].TrimStart();
        if (firstLine.StartsWith('{') || firstLine.StartsWith('[')) return "json";

        // Check for Markdown
        var markdownHeadings = sample.Count(l => Regex.IsMatch(l, @"^\s*#"));
        if (markdownHeadings >= 3) return "markdown";

        // Check for Python
        var pyDefs = sample.Count(l => Regex.IsMatch(l, @"^\s*(def|class)\s"));
        if (pyDefs >= 2) return "python";

        // Check for C-style
        var cStyleDefs = sample.Count(l => Regex.IsMatch(l, @"\bfunction\b|\bclass\b|\binterface\b|\{\s*$"));
        if (cStyleDefs >= 2) return "cstyle";

        // Check for Log
        var logStarts = sample.Count(l => 
            Regex.IsMatch(l, @"^\s*\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}:\d{2}") ||
            Regex.IsMatch(l, @"^\s*\d{2}:\d{2}:\d{2}"));
        if (logStarts >= Math.Ceiling(sample.Length * 0.3)) return "log";

        return "text";
    }

    public List<string[]> SplitFile(string content, string fileName, int maxBytes)
    {
        var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var kind = GetFileKind(fileName, lines);
        
        var blocks = kind switch
        {
            "cstyle" or "js" => SplitCStyle(lines),
            "python" or "powershell" or "ruby" => SplitPythonLike(lines),
            "markdown" => SplitMarkdown(lines),
            "json" or "ndjson" => SplitJson(lines),
            "log" => SplitLog(lines),
            _ => SplitPlainText(lines)
        };

        return SplitBySize(blocks, maxBytes);
    }

    private List<string[]> SplitCStyle(string[] lines)
    {
        var blocks = new List<string[]>();
        var current = new List<string>();
        var braceDepth = 0;

        foreach (var line in lines)
        {
            var trim = line.Trim();
            if (Regex.IsMatch(trim, @"^\s*(class|struct|enum|interface)\b") ||
                Regex.IsMatch(trim, @"^\s*(public|private|protected|static|\w+\s+)*\w+\s*\([^;]*\)\s*\{?$"))
            {
                if (current.Count > 0)
                {
                    blocks.Add(current.ToArray());
                    current = new List<string>();
                }
            }

            current.Add(line);
            braceDepth += line.Count(c => c == '{');
            braceDepth -= line.Count(c => c == '}');

            if (braceDepth == 0 && current.Count > 0 && trim.Contains('}'))
            {
                blocks.Add(current.ToArray());
                current = new List<string>();
            }
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitPythonLike(string[] lines)
    {
        var blocks = new List<string[]>();
        var current = new List<string>();

        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"^(def|class|function)\s"))
            {
                if (current.Count > 0)
                {
                    blocks.Add(current.ToArray());
                    current = new List<string>();
                }
            }
            current.Add(line);
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitMarkdown(string[] lines)
    {
        var blocks = new List<string[]>();
        var current = new List<string>();

        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"^\s*#"))
            {
                if (current.Count > 0)
                {
                    blocks.Add(current.ToArray());
                    current = new List<string>();
                }
            }
            current.Add(line);
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitJson(string[] lines)
    {
        var nonEmpty = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
        
        // Check for NDJSON
        if (nonEmpty.Length > 0)
        {
            var ndjsonRatio = nonEmpty.Count(l => 
                Regex.IsMatch(l, @"^[\s\t]*[\{\[].*[\}\]]\s*,?\s*$")) / (double)nonEmpty.Length;
            
            if (ndjsonRatio >= 0.6)
            {
                return nonEmpty.Select(l => new[] { l }).ToList();
            }
        }

        var blocks = new List<string[]>();
        var current = new List<string>();
        var depth = 0;

        foreach (var line in lines)
        {
            current.Add(line);
            var inString = false;
            var escape = false;

            foreach (var ch in line)
            {
                if (escape) { escape = false; continue; }
                if (ch == '\\') { escape = true; continue; }
                if (ch == '"') { inString = !inString; continue; }
                if (inString) continue;
                if (ch == '{' || ch == '[') depth++;
                else if (ch == '}' || ch == ']') depth--;
            }

            if (depth <= 0 && current.Count > 0)
            {
                blocks.Add(current.ToArray());
                current = new List<string>();
                depth = 0;
            }
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitLog(string[] lines)
    {
        var blocks = new List<string[]>();
        var current = new List<string>();

        foreach (var line in lines)
        {
            var isStart = Regex.IsMatch(line, @"^\s*\d{4}-\d{2}-\d{2}[ T]\d{2}:\d{2}:\d{2}") ||
                          Regex.IsMatch(line, @"^\s*\d{2}:\d{2}:\d{2}") ||
                          Regex.IsMatch(line, @"^\s*\[\d{4}-\d{2}-\d{2}");

            if (isStart && current.Count > 0)
            {
                blocks.Add(current.ToArray());
                current = new List<string>();
            }
            current.Add(line);
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitPlainText(string[] lines)
    {
        var blocks = new List<string[]>();
        var current = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (current.Count > 0)
                {
                    blocks.Add(current.ToArray());
                    current = new List<string>();
                }
                continue;
            }
            current.Add(line);
        }

        if (current.Count > 0) blocks.Add(current.ToArray());
        return blocks;
    }

    private List<string[]> SplitBySize(List<string[]> blocks, int maxBytes)
    {
        var chunks = new List<string[]>();
        var current = new List<string>();
        var currentBytes = 0;

        foreach (var block in blocks)
        {
            var blockText = string.Join("\n", block);
            var blockBytes = Encoding.UTF8.GetByteCount(blockText);

            if (blockBytes > maxBytes)
            {
                // Block is too large, split by lines
                var temp = new List<string>();
                var tempBytes = 0;

                foreach (var ln in block)
                {
                    var lnBytes = Encoding.UTF8.GetByteCount(ln + "\n");
                    if (tempBytes + lnBytes > maxBytes && temp.Count > 0)
                    {
                        chunks.Add(temp.ToArray());
                        temp = new List<string>();
                        tempBytes = 0;
                    }
                    temp.Add(ln);
                    tempBytes += lnBytes;
                }

                if (temp.Count > 0) chunks.Add(temp.ToArray());
                continue;
            }

            if (currentBytes + blockBytes <= maxBytes)
            {
                current.AddRange(block);
                currentBytes += blockBytes;
            }
            else
            {
                if (current.Count > 0) chunks.Add(current.ToArray());
                current = new List<string>(block);
                currentBytes = blockBytes;
            }
        }

        if (current.Count > 0) chunks.Add(current.ToArray());
        return chunks;
    }

    public string MergeFiles(List<(string Name, string Content)> files, bool addHeaders)
    {
        var sb = new StringBuilder();

        foreach (var (name, content) in files)
        {
            if (addHeaders)
            {
                sb.AppendLine($"----- {name} -----");
            }
            sb.AppendLine(content);
        }

        return sb.ToString();
    }

    public (string NewName, string Content) ConvertToText(string fileName, string content)
    {
        var baseName = Path.GetFileNameWithoutExtension(fileName);
        var newName = $"{baseName}.txt";
        return (newName, content);
    }
}
