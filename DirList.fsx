module DirList =

    open System
    open System.IO

    // Format file permissions similar to ls -l
    let private formatPermissions (fi: FileSystemInfo) =
        let attrs = fi.Attributes
        let isDir = (attrs &&& FileAttributes.Directory) = FileAttributes.Directory

        let rwx r w x =
            sprintf "%c%c%c"
                (if r then 'r' else '-')
                (if w then 'w' else '-')
                (if x then 'x' else '-')

        // Simplified permission model (Linux-style look)
        let perms = rwx true true true
        (if isDir then "d" else "-") + perms + perms + perms

    // Print ls -l style listing
    let handleLs (args: string[]) =
        let path =
            if args.Length > 0 then args.[0]
            else Environment.CurrentDirectory

        try
            let dir = DirectoryInfo(path)

            if not dir.Exists then
                printfn "ls: cannot access '%s': No such directory" path
            else
                for item in dir.GetFileSystemInfos() do
                    let perms = formatPermissions item
                    let size =
                        match item with
                        | :? FileInfo as f -> f.Length
                        | _ -> 0L

                    let time = item.LastWriteTime.ToString("yyyy-MM-dd HH:mm")
                    printfn "%s %10d %s %s"
                        perms size time item.Name
        with ex ->
            printfn "ls error: %s" ex.Message

    // Recursive tree printing
    let handleTree (args: string[]) =
        let root =
            if args.Length > 0 then args.[0]
            else Environment.CurrentDirectory

        let rec printTree (dir: DirectoryInfo) (indent: string) =
            let items = dir.GetFileSystemInfos()
            let lastIndex = items.Length - 1

            for i = 0 to lastIndex do
                let item = items.[i]
                let isLast = i = lastIndex
                let prefix = if isLast then "└── " else "├── "

                printfn "%s%s%s" indent prefix item.Name

                match item with
                | :? DirectoryInfo as d ->
                    let newIndent =
                        indent + (if isLast then "    " else "│   ")
                    printTree d newIndent
                | _ -> ()

        try
            let dir = DirectoryInfo(root)
            printfn "%s" dir.FullName
            printTree dir ""
        with ex ->
            printfn "tree error: %s" ex.Message



