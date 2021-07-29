# LogViewer_New
This is the updated version of a LogViewer utility that I developed in my spare time for my client while working at NuWest. I noticed that they had log entries in many locations in many formats but that they had no way to view them all in one application and to see changes in real time. I therefore enthusiastically began to develop a custom application that would meet their needs.

The LogViewer utility:
- Is a multi-threaded WPF desktop application built with C#'s task-based multi-threading libraries
- Uses ReactiveUI's open source DynamicData, a collections-based library built on RX.NET
- Uses MahApps.Metro, an open source WPF library for creating modern WPF user interfaces
- Uses a custom library that I built which contains logging operations
  * The logging API logs asynchronously to a remote location (if available)
  * If the remote log location is unavailable, logs are stored locally and uploaded when connection is restored
- Enables my client to view the log entries from all of their SQL stored procedures which are stored in a custom SQL table
- Allowed my client to aggregate log entries from multiple sources (e.g. custom SQL log tables and client log files) into a single 'LogView' which gives the user a more comprehensive picture of the health of their system
- Allowed my client to create multiple LogViews, giving them the ability to view multiple combinations of log sources at the same time
- Gave my client the option to see log entries in real time through auto-refresh functionality

The home screen of the application allows users to open a LogView which contains log entries from one of two sources: tables in a SQL database or .txt log files.
![image](https://user-images.githubusercontent.com/61886068/127565146-3f1e679c-bcfd-48b1-a488-75b12d78f906.png)

An open LogView appears like this:
![image](https://user-images.githubusercontent.com/61886068/127565417-71dcdd76-8d4e-4911-af83-88a0f458bf1a.png)

Once open, the source(s) of the log entries can be seen and edited on the right and the log entries can be filtered using the options at the top.

Additionally, each log view has two additional settings: the option to automatically refresh the log entries from the sources and the option to highlight new log entries.
![image](https://user-images.githubusercontent.com/61886068/127565722-36c96107-ad00-418e-a4e0-9a595d37e382.png)

