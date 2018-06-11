# ServiceBusDuplicateDetection
###### tags: `Azure` `WebJobs` `Service Bus` `Redis` `Duplicate Detection` `Cron`
`Problem:` �b�ثe�B�z���t�Τ��A�]���u�@�Ƶ{���ɶ��|����䥦�]���ӧ�ʡA�ҥH�C������Ƶ{���|�������O���ƪ��u�@�A�]���ɶ��Y�����ʻݭn�H�s�����D�A�M Service Bus duplicat detection ������O�ۤϪ��C

`Test:`
- ServiceBusDuplicateMessageSender:
    - �o�O�@�� WebJob �M�סA�]�O�����ժ��D��C�̷ӱƵ{�b�C�Ӥp�ɪ����I����A�|�Ƶ{���Ө�p�ɪ��u�@�A�G�C������|���@�p�ɪ��u�@�Ƶ{�O���ƪ��C
    - �ϥ� WebJobs SDK �i�� cron ���Ƶ{�C
    - �N�T�� enqueue �� Service Bus ��A�ϥ� Redis �Ȧs sequence number �H�Q�U���Ƶ{�B�z���ưT���C
    - �ϥ� slack4net �� console monitor�C

- ServiceBusMessageReceiver:
    - �o�O�@�� WebJob �M�סA�D�n�O�Ψ��[��Ƥu�@�Ƶ{�����T�ʡC
    - �ϥ� Service Bus Trigger �ӱ����T���C
    - �ϥ� slack4net �� console monitor�C

`Solution:` �ݰ_�ӹB�@�}�n�C