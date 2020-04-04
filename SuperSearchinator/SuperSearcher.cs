using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperSearchinator
{
    public class SuperSearcher
    {
        private IWordIndex _index { get; set; }
        private FileCollector FileCollector { get; set; }
        private DictionaryIndexBuilder IndexBuilder { get; set; }
        public SuperSearcher()
        {
            IndexBuilder = new DictionaryIndexBuilder(EqualityComparer<string>.Default);
        }
        public List<FileInfo> GetFiles() => FileCollector.Files;
        public List<string> GetFileNames()
        {
            List<string> filePaths = new List<string>();
            foreach (FileInfo file in GetFiles())
                filePaths.Add(file.FullName);

            return filePaths;
        }
        public void AddFiles(string path)
        {
            FileCollector = new FileCollector(path);
            _index = IndexBuilder.BuildIndexFromFiles(FileCollector.Files);
        }
        public List<SearchResult> Search(string word)
        {
            List<SearchResult> searchResults = new List<SearchResult>();
            foreach (WordLocation wordLocation in _index.Find(word).Locations)
            {
                searchResults.Add(new SearchResult(wordLocation, word));
            }
            return searchResults;
        }
    }
    public struct SearchResult
    {
        public readonly string Word { get; }
        public readonly int WordIndex { get; }   // index within the line.
        public readonly int LineNumber { get; } // line within the file.
        public readonly string FileName { get;} // file containing the word.
        public readonly string LineContent { get;}
        public SearchResult(WordLocation wordLocation,string word)
        {
            Word = word;
            FileName = wordLocation.FileName;
            LineNumber = wordLocation.LineNumber;
            WordIndex = wordLocation.WordIndex;
            LineContent = GetLine(FileName,LineNumber);
        }
        private static string GetLine(string filePath, int lineNumber)
        {
            string content = null;
            try
            {
                using (StreamReader file = new StreamReader(filePath))
                {
                    for (int i = 1; i < lineNumber; i++)
                    {
                        file.ReadLine();

                        if (file.EndOfStream)
                        {
                            Console.WriteLine($"End of file.  The file only contains {i} lines.");
                            break;
                        }
                    }
                    content = file.ReadLine();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("There was an error reading the file: ");
                Console.WriteLine(e.Message);
            }
            return content;
        }
        public override string ToString()
        {
            return Word + " was found in " + FileName + "(" + LineNumber + ":" + WordIndex + ")"
                + "\t\t" + LineContent;
        }
    }
    public struct WordLocation
    {
        public WordLocation(string fileName, int lineNumber, int wordIndex)
        {
            FileName = fileName;
            LineNumber = lineNumber;
            WordIndex = wordIndex;
        }
        public readonly string FileName { get; } // file containing the word.
        public readonly int LineNumber { get; } // line within the file.
        public readonly int WordIndex { get; }   // index within the line.

    }
    public struct WordOccurrences
    {
        private WordOccurrences(int nOccurrences, WordLocation[] locations)
        {
            NumberOfOccurrences = nOccurrences;
            Locations = locations;
        }

        public static readonly WordOccurrences None = new WordOccurrences(0, new WordLocation[0]);

        public static WordOccurrences FirstOccurrence(string fileName, int lineNumber, int wordIndex)
        {
            return new WordOccurrences(1, new[] { new WordLocation(fileName, lineNumber, wordIndex) });
        }

        public WordOccurrences AddOccurrence(string fileName, int lineNumber, int wordIndex)
        {
            return new WordOccurrences(
                NumberOfOccurrences + 1,
                Locations
                    .Concat(
                        new[] { new WordLocation(fileName, lineNumber, wordIndex) })
                    .ToArray());
        }

        public readonly int NumberOfOccurrences;
        public readonly WordLocation[] Locations;
    }
    public interface IWordIndexBuilder
    {
        void AddWordOccurrence(string word, string fileName, int lineNumber, int wordIndex);
        IWordIndex Build();
    }
    public interface IWordIndex
    {
        WordOccurrences Find(string word);
    }
    public class DictionaryIndexBuilder : IWordIndexBuilder
    {
        private Dictionary<string, WordOccurrences> _dict;

        private class DictionaryIndex : IWordIndex
        {
            private readonly Dictionary<string, WordOccurrences> _dict;

            public DictionaryIndex(Dictionary<string, WordOccurrences> dict)
            {
                _dict = dict;
            }
            public WordOccurrences Find(string word)
            {
                WordOccurrences found;
                if (_dict.TryGetValue(word, out found))
                    return found;
                return WordOccurrences.None;
            }
        }

        public DictionaryIndexBuilder(IEqualityComparer<string> comparer)
        {
            _dict = new Dictionary<string, WordOccurrences>(comparer);
        }
        public void AddWordOccurrence(string word, string fileName, int lineNumber, int wordIndex)
        {
            WordOccurrences current;
            if (!_dict.TryGetValue(word, out current))
                _dict[word] = WordOccurrences.FirstOccurrence(fileName, lineNumber, wordIndex);
            else
                _dict[word] = current.AddOccurrence(fileName, lineNumber, wordIndex);
        }
        public IWordIndex Build()
        {
            var dict = _dict;
            _dict = null;
            return new DictionaryIndex(dict);
        }
    }
    public static class BuilderExtensions
    {
        public static IWordIndex BuildIndexFromFiles(this IWordIndexBuilder builder, IEnumerable<FileInfo> wordFiles)
        {
            var wordSeparators = new char[] { ',', ' ', '\t', ';' /* etc */ };
            foreach (var file in wordFiles)
            {
                var lineNumber = 1;
                using (var reader = file.OpenText())
                {
                    while (!reader.EndOfStream)
                    {
                        var words = reader
                             .ReadLine()
                             .Split(wordSeparators, StringSplitOptions.RemoveEmptyEntries)
                             .Select(f => f.Trim());

                        var wordIndex = 1;
                        foreach (var word in words)
                            builder.AddWordOccurrence(word, file.FullName, lineNumber, wordIndex++);

                        lineNumber++;
                    }
                }
            }
            return builder.Build();
        }
    }
}
