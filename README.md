# ServiceBusDuplicateDetection
###### tags: `Azure` `WebJobs` `Service Bus` `Redis` `Duplicate Detection` `Cron` `Abadon` `Sequence Number`
`Problem:` 在目前處理的系統中，因為工作排程的時間會受到其它因素而更動，所以每次執行排程都會有部份是重複的工作，因為時間若有異動需要以新的為主，和 Service Bus duplicat detection 的機制是相反的。

`Project:`
- ServiceBusDuplicateMessageSender:
    - 這是一個 WebJob 專案，也是本測試的主體。依照排程在每個小時的整點執行，會排程未來兩小時的工作，故每次執行會有一小時的工作排程是重複的。
    - 使用 WebJobs SDK 進行 cron 的排程。
    - 將訊息 enqueue 至 Service Bus 後，使用 Redis 暫存 sequence number、message id 以及排程的時間，以利下次排程處理決定是否要更新訊息內容。
    - 使用 slack4net 當做 console monitor。

- ServiceBusMessageReceiver:
    - 這是一個 WebJob 專案，主要是用來觀察重複工作排程的正確性。
    - 使用 Service Bus Trigger 來接收訊息。
    - 使用 slack4net 當做 console monitor。

`Test:` 
- 使用 BrokerMessage 可以比較方便調用 .Abadon 延伸方法
- 使用 string 當做 message body，若需要傳遞物件，可以先用 JsonConverter 序列化後，放進 BrokerMessage 會比較好處理。
- 要很注意程式的正確性，若有未處理的 exception，message 會不斷地 enqueue 導致效能問題。
- Azure Service Bus 支援高併發 (concurrent) 的模式，可以使用 AsParallel 然後平行 Enqueue，可以節省很多時間。