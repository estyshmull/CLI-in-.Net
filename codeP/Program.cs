using System.CommandLine;
using System.CommandLine.Parsing;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

List<string> filesToMove = new List<string>();
var bundle = new Command("bundle", "bundle code files to single file");
var createRsp = new Command("create-rsp", "ficher for bundle");

var rootbundle = new RootCommand("root command for file to CLI");
var langOption = new Option<string>("--lang", "all language to choise");
var output = new Option<FileInfo>("--out");
var addNote = new Option<bool>("--n", "if you want note");
var sortOption = new Option<bool>("--s", "sort files by file type"); // New sortOption
var removeEmptyLinesOption = new Option<bool>("--rel", "remove empty lines from source code");
var addAuthor = new Option<string>("--a", "add author to the files");

bundle.AddOption(removeEmptyLinesOption);
bundle.AddOption(sortOption); // Added sortOption
bundle.AddOption(output);
bundle.AddOption(addAuthor);
bundle.AddOption(langOption);
bundle.AddOption(addNote);

bundle.SetHandler((lgs, out1, note, sort, removeEmptyLines, aother) =>
{
    bool f = false;
    string content = "";
    var langs = lgs.ToString();
    var langList = langs.Split(",").ToList();

    if (langs == "all")
    {
        filesToMove.AddRange(Directory.GetFiles(".", "*"));
    }
    else
    {
        try
        {
            foreach (var file in langList)
            {
                filesToMove.AddRange(Directory.GetFiles(".", "*." + file));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: ther is folder or more that invalid");
        }

    }
    if (sort)
    {
        // Sort files by file type
        filesToMove.Sort((file1, file2) =>
        {
            var fileType1 = Path.GetExtension(file1);
            var fileType2 = Path.GetExtension(file2);
            return string.Compare(fileType1, fileType2);
        });
    }
    Console.WriteLine(filesToMove.Count);

    foreach (var file in filesToMove)
    {
        if (removeEmptyLines)
        {
            // קריאת הקובץ
            string content2 = File.ReadAllText(file);

            // מיון הקבצים
            string[] lines = content2.Split('\n');
            var filteredLines = lines.Where(line => line.Trim().Length > 0);

            // החלפת השורות הממוינות בקובץ
            File.WriteAllText(file, string.Join("\n", filteredLines));
            content2 = File.ReadAllText(file);
        }
        using (var reader = new StreamReader(file))
        {
            // Read the content of the file
            content += reader.ReadToEnd();
            if (out1 != null)
            {
                string filePath = out1.FullName;
                try
                {
                    using (StreamWriter writer = new StreamWriter(filePath, append: true))
                    {
                        if (note)
                        {
                            string currentDirectoryNote = Environment.CurrentDirectory;
                            writer.Write("מקור: " + currentDirectoryNote + "\n name of file is: " + file + "\n");
                            Console.WriteLine(currentDirectoryNote + "name" + file);
                        }
                        if (aother != null && f == false)
                        {
                            f = true;
                            string name = aother.ToString();
                            writer.Write("name auther :" + name + "\n");
                        }
                        writer.Write(content);
                    }
                }
                catch (DirectoryNotFoundException ex)
                {
                    Console.WriteLine("ERROR: the directory not found");
                }
            }
        }
    }

    Console.WriteLine("Files moved successfully");
}, langOption, output, addNote, sortOption, removeEmptyLinesOption, addAuthor);
createRsp.SetHandler(() =>
{
    string command = "codeP bundle ";
    Console.WriteLine("If you want certain languages, write them whith ',' between the languages, if not, then write 'all'");
    command += "--lang " + Console.ReadLine() + " ";
    Console.WriteLine("if you want output?:y/n");
    bool o=Console.ReadLine()=="y"? true:false;
    Console.WriteLine("You can enter a path to a folder or just a folder name and the file will be saved in the current folder");
    FileInfo file = new FileInfo(Console.ReadLine());
    if (o)
    {
        command += "--out \"" + file.FullName;
        command += "\"";
        Console.WriteLine("If you want a note with the source of the files and the name enter:y/n");
        if (Console.ReadLine() == "y")
        {
            command += " --n ";
        }
        Console.WriteLine("To sort files by file type enter:y/n");
        if (Console.ReadLine() == "y")
        {
            command += "--s ";
        }
        Console.WriteLine("To add author to the files enter name");
        if (Console.ReadLine() == "y")
        {
            Console.WriteLine("enter name");
            command += "--a " + Console.ReadLine() + " ";
        }
    }

    Console.WriteLine("To remove empty lines from source code enter:y/n");
    if (Console.ReadLine() == "y")
    {
        command += "--rel ";
    }

    var projectDirectory = Directory.GetCurrentDirectory();
    StreamWriter writer = new StreamWriter(projectDirectory + "\\" + "@codP.txt");
    writer.WriteLine(command);
    Console.WriteLine(command);
    Console.WriteLine("create responsFile!!!!!!!!!!!!!!!!!!!!!!!!!!");
    writer.Close();

});


rootbundle.AddCommand(bundle);
rootbundle.AddCommand(createRsp);
rootbundle.InvokeAsync(args);


