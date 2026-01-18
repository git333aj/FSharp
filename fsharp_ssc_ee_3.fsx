//state machine|CancellationTokenSource()

open System
open System.Threading
open System.Threading.Tasks

let fakeDownload (ct: CancellationToken) =
    task {
        printfn "State 0: Starting download"

        do! Task.Delay(1000, ct)
        printfn "State 1: 50%% downloaded"

        do! Task.Delay(1000, ct)
        printfn "State 2: Download finished"
    }

printfn "Calling fakeDownload"

let cts = new CancellationTokenSource()

let t = fakeDownload cts.Token

printfn "Doing other work..."
Task.Delay(1500).Wait()   // simulate other work

// Uncomment next line to test cancellation
// cts.Cancel()

try
    t.Wait()
    printfn "All done"
with
    | :? AggregateException as ex ->
    match ex.InnerException with
    | :? TaskCanceledException ->
        printfn "Download was cancelled"
    | _ ->
        printfn "Other error: %s" ex.Message

