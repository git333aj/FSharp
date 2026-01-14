open System
open System.IO

/// Show metadata for a file or directory
let showStat path =
    if File.Exists path then
        let f = FileInfo path
        printfn "File: %s" f.FullName
        printfn "Size: %d bytes" f.Length
        printfn "Last modified: %s" (f.LastWriteTime.ToString())
        printfn "Readable: %b" true
        printfn "Writable: %b" (not f.IsReadOnly)

    elif Directory.Exists path then
        let d = DirectoryInfo path
        printfn "Directory: %s" d.FullName
        printfn "Last modified: %s" (d.LastWriteTime.ToString())

    else
        printfn "stat: cannot stat '%s': No such file or directory" path

/// Get command line argument from FSI
let args =
    if fsi.CommandLineArgs.Length > 1 then
        fsi.CommandLineArgs.[1..]  // skip script name
    else
        [||]

match args with
| [| path |] -> showStat path
| _ -> printfn "Usage: dotnet fsi stat.fsx <file-or-directory>"

