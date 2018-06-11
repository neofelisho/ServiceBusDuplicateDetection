# ServiceBusDuplicateDetection
###### tags: `Azure` `WebJobs` `Service Bus` `Redis` `Duplicate Detection` `Cron`
`Problem:` 在目前處理的系統中，因為工作排程的時間會受到其它因素而更動，所以每次執行排程都會有部份是重複的工作，因為時間若有異動需要以新的為主，和 Service Bus duplicat detection 的機制是相反的。

`Test:`
- ServiceBusDuplicateMessageSender:
    - 這是一個 WebJob 專案，也是本測試的主體。依照排程在每個小時的整點執行，會排程未來兩小時的工作，故每次執行會有一小時的工作排程是重複的。
    - 使用 WebJobs SDK 進行 cron 的排程。
    - 將訊息 enqueue 至 Service Bus 後，使用 Redis 暫存 sequence number 以利下次排程處理重複訊息。
    - 使用 slack4net 當做 console monitor。

- ServiceBusMessageReceiver:
    - 這是一個 WebJob 專案，主要是用來觀察重複工作排程的正確性。
    - 使用 Service Bus Trigger 來接收訊息。
    - 使用 slack4net 當做 console monitor。

`Solution:` 看起來運作良好。