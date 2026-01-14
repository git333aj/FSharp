open System
open System.IO
open System.Net.Http
open System.Threading
open System.Threading.Tasks

// ──────────────── Shared state ────────────────
//obj() creates new reference object,no data, used as a lock, allow one thread per download ( from multiple thread  and multiple download)
let consoleLock = obj()
let mutable completedCount = 0

// ──────────────── Progress helper ────────────────

let drawProgressBar completed total =
    let barWidth = 30
    let filled = (completed * barWidth) / total
    let bar =
        String.replicate filled "█" +
        String.replicate (barWidth - filled) "─"

    lock consoleLock (fun () ->
        Console.SetCursorPosition(0, Console.CursorTop)
        printf "[%s] %d / %d" bar completed total
        if completed = total then printfn ""
    )

let printProgress total =
(* 
   Interlocked is part of namespace-System.Threading.Interlocked.It provides atomic operation on shared variables.Increment() and Decrement() are methods.no two thread can update the variable at same time.Interlocked.Increment(&completedCount) atomically add 1 to completedCount and return new value.Interlocked.Increment() expect a reference to the variable , not the value-&completCount.Atomic-only one thread can increment at a time)
   
*)

    let doneCount = Interlocked.Increment(&completedCount)
    drawProgressBar doneCount total

// ──────────────── Configuration ────────────────

let baseDir = "ssc_je_demo"
Directory.CreateDirectory(baseDir) |> ignore
//list literal -[a.pdf;b.pdf;c.pdf].f# list are immutable and allows duplicate.
let pdfUrls =
    [
        "https://www.pce-fet.com/common/library/books/39/173_BasicElectricalEngineeringbyV.K.MehtaandRohitMehta.pdf"
        "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-05-jun-2024-shift-3.pdf"
        "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-06-jun-2024-shift-2.pdf"
        "https://s3.ap-south-1.amazonaws.com/www.careerpower.in/2021/58159.pdf"
        "https://mrcet.com/downloads/digital_notes/HS/Basic%20Electrical%20Engineering%20R-20.pdf"
        "https://www.ittchoudwar.org/upload/CNT%20NOTE%20ITT.pdf"
        "https://kanchiuniv.ac.in/wp-content/uploads/2022/02/POWER-SYSTEMS.pdf"
        "https://d6s74no67skb0.cloudfront.net/course-material/EE601-Basic-Electrical-and-DC-Theory.pdf"
        "https://example-files.online-convert.com/document/pdf/example.pdf"
        "https://gate-notes.anandankitkumar.in/2019/09/ssc-je-study-material-pdf-junior.html"
        "https://cf.madeeasypublications.org/postal/Uploads/postal-books/EE/sample/1858.pdf"
    ]
    (* remove duplicate . lazy sequence are computed on demand.Seq is library module, comes under microsoft.fsharp.collection namespace.seq is a keyword.distinct is a helper function-Seq.map|Seq.filter etc *)
    |> Seq.distinct
    //convert to array,later Array.map needs an array
    |> Seq.toArray

let totalCount = pdfUrls.Length

let maxParallel = 3
let semaphore = new SemaphoreSlim(maxParallel)

// ──────────────── Shared HttpClient ────────────────

let client = new HttpClient()
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0")

// ──────────────── Download function ────────────────

let downloadPdf (url: string) =
    task {
        do! semaphore.WaitAsync()
        try
            let uri = Uri(url)
            let fileName = Path.GetFileName(uri.AbsolutePath).Replace("%20", " ")
            let filePath = Path.Combine(baseDir, fileName)

            if File.Exists(filePath) then
                lock consoleLock (fun () ->
                    printfn "↺ Already exists: %s" fileName
                )
            else
                try
                    let! bytes = client.GetByteArrayAsync(uri)
                    File.WriteAllBytes(filePath, bytes)
                    lock consoleLock (fun () ->
                    printfn "✔ Downloaded: %s" fileName
                )
                with ex ->
                //only one thread at a time can enter into this block
                    lock consoleLock (fun () ->
                    printfn "✖ Failed: %s -> %s" url ex.Message
            )
        finally
            printProgress totalCount
            semaphore.Release() |> ignore
    }

// ──────────────── Main execution ────────────────

pdfUrls
|> Array.map downloadPdf
|> Task.WhenAll
|> fun t -> t.Wait()

printfn "All done!"

