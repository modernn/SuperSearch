using System;
using System.Collections.Generic;
using System.IO;

namespace SuperSearchinator
{
    public class Program
    {
        public static SuperSearcher SuperSearcher;
        static void Main(string[] args)
        {
            SuperSearcher = new SuperSearcher();
            SuperSearcher.AddFiles(GetPath());
            Console.WriteLine("Importing the following files: ");
            PrintFilePaths();
            Search();
        }
        private static void PrintFilePaths()
        {
            foreach (string path in SuperSearcher.GetFileNames())
            {
                Console.WriteLine(path);
            }
        }
        private static string GetPath()
        {
            Console.WriteLine("Please enter the path to your file/directory:");
            return Console.ReadLine();
        }
        private static void Search()
        {
            Console.WriteLine("Please enter the word you'd like to search for: ");
            foreach (SearchResult searchResult in SuperSearcher.Search(Console.ReadLine()))
            {
                Console.WriteLine(searchResult.ToString());
            }
        }
        private static void Print(string toPrint) => Console.WriteLine(toPrint);
    }
}
