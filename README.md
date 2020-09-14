# Tensile
Tessile is a project for testing Microsoft asynchronous socket programming. It includes three projects:
  - SensorAsync(server)
  - DashboardAsync(client)
  - MiniTensile(client)
## SensorAsync(server)
This is a console application based on [Microsoft Asynchronous Server Socket](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-server-socket-example) example.
## DashboardAsync(cleint)
It is another console project and it is based on [Microsoft Asynchronous Client Socket](https://docs.microsoft.com/en-us/dotnet/framework/network-programming/asynchronous-client-socket-example) example. I started with this program to know how to work with asynchronous socket programming.
## MiniTensile(clinet)
This one is a WPF program based on DashboardAsync project. It sends commands to SensorAsync(server) and receive data, parse and after that show them at UI. I used Telerik component to show data. 
## Performance
By using asynchronous socket programming I could handle **1200 message per second**.

This is the result of running the project:

![minitensile runtime image](https://github.com/ahmadasm/Tensile/blob/master/minitensile.jpg)


