open System
open System.IO
open System.Net.Http
open System.Threading
open System.Threading.Tasks

// ──────────────── Configuration ────────────────

let baseDir = "ssc_je_ee"
Directory.CreateDirectory(baseDir) |> ignore

let pdfUrls = [
    "https://www.pce-fet.com/common/library/books/39/173_BasicElectricalEngineeringbyV.K.MehtaandRohitMehta.pdf";
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-05-jun-2024-shift-3.pdf";
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-06-jun-2024-shift-2.pdf";
    "https://s3.ap-south-1.amazonaws.com/www.careerpower.in/2021/58159.pdf";
    "https://mrcet.com/downloads/digital_notes/HS/Basic%20Electrical%20Engineering%20R-20.pdf"
    "https://www.pce-fet.com/common/library/books/39/173_BasicElectricalEngineeringbyV.K.MehtaandRohitMehta.pdf";
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-05-jun-2024-shift-3.pdf";
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-06-jun-2024-shift-2.pdf";
    "https://s3.ap-south-1.amazonaws.com/www.careerpower.in/2021/58159.pdf";
    "https://mrcet.com/downloads/digital_notes/HS/Basic%20Electrical%20Engineering%20R-20.pdf";
    "https://www.ittchoudwar.org/upload/CNT%20NOTE%20ITT.pdf";
    "https://kanchiuniv.ac.in/wp-content/uploads/2022/02/POWER-SYSTEMS.pdf";
    "https://d6s74no67skb0.cloudfront.net/course-material/EE601-Basic-Electrical-and-DC-Theory.pdf";
    "https://www.adda247.com/jobs/wp-content/uploads/sites/12/2025/06/24122929/SSC-JE-Electrical-Question-Paper-28.October.2020-1st-Shift.pdf?srsltid=AfmBOorM5zc73vr8mXyeUMCOSDoutJ2kkVP_KgznmLERhUTi4Wxr9gAw";
    "https://example-files.online-convert.com/document/pdf/example.pdf"
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-05-jun-2024-shift-3.pdf"
    "https://sscportal.in/sites/default/files/ssc-je-paper-2024-electrical-engineering-06-jun-2024-shift-2.pdf"
    "https://s3.ap-south-1.amazonaws.com/www.careerpower.in/2021/58159.pdf"
    "https://gate-notes.anandankitkumar.in/2019/09/ssc-je-study-material-pdf-junior.html" // (page; extract actual .pdf)
    "https://cf.madeeasypublications.org/postal/Uploads/postal-books/EE/sample/1858.pdf" // sample practice book by Made Easy :contentReference[oaicite:6]{index=6}
]

let maxParallel = 3
let semaphore = new SemaphoreSlim(maxParallel)

// ──────────────── Shared HttpClient ────────────────

let client = new HttpClient()
client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0")

// ──────────────── Download function ────────────────

let downloadPdf (url: string) =
    task {
        do! semaphore.WaitAsync()  // Acquire slot

        try
            let uri = Uri(url)
            let fileName = Path.GetFileName(uri.AbsolutePath).Replace("%20", " ")
            let filePath = Path.Combine(baseDir, fileName)

            if File.Exists(filePath) then
                printfn "↺ Already exists: %s" fileName
            else
                try
                    let! bytes = client.GetByteArrayAsync(uri)
                    File.WriteAllBytes(filePath, bytes)
                    printfn "✔ Downloaded: %s" fileName
                with ex ->
                    printfn "✖ Failed: %s -> %s" url ex.Message
        finally
            semaphore.Release() |> ignore  // Release slot
    }

// ──────────────── Main execution ────────────────

let mainTask =
    pdfUrls
    |> Seq.map downloadPdf
    |> Seq.toArray
    |> Task.WhenAll

mainTask.Wait()
printfn "All done!"

