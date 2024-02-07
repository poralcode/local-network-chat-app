# local-network-chat-app
# Simple Chat Application using Network Socket Programming in C#

## Project Overview
The goal of this project is to develop a basic chat application in C# that allows communication between a server and multiple clients over a local network. The application will utilize both TCP/IP and UDP protocols to establish connections, enabling automatic server discovery for clients and seamless message exchange.

## Key Features
- **Server-Client Architecture**: The application will offer the flexibility to function as either a server or a client upon startup, providing options for initiating connections or listening for incoming connections.
- **Automatic Server Discovery**: The server will utilize UDP broadcasting to announce its presence on the local network. Clients will employ UDP listening to automatically discover the server without explicitly specifying its address.
- **Message Exchange**: Once connected, the server and client(s) will be able to exchange text-based messages in real-time, enabling a basic chat interface.

## Technical Implementation
- **Socket Programming in C#**: Utilize the .NET Socket class for network communication. TCP/IP will be employed for reliable, stream-oriented communication between the server and clients, ensuring message delivery.
- **Combining TCP/IP and UDP**: TCP/IP will be used for establishing connections and maintaining communication, while UDP will facilitate the initial server discovery process for clients through broadcasting and listening for server announcements.
- **Multi-threading**: Implement multi-threading to handle simultaneous connections from multiple clients and ensure the responsiveness of the application during message exchange.

## Technologies Used
- **APP Name**: local-network-chat-app
- **Material Design in Xaml**: http://materialdesigninxaml.net/
- **WPF**
- **C#**

## Sample Output
Sample output located in: `bin/Release/net6.0-windows`
Tested on Windows 11

## Screenshots
## Client
![Alt text](/../main/Screenshots/Client%20-%201.png?raw=true "Optional Title")
![Alt text](/../main/Screenshots/Client%20-%202.png?raw=true "Optional Title")
![Alt text](/../main/Screenshots/Client%20-%203.png?raw=true "Optional Title")

## Conclusion
This project aims to create a functional chat application using C# and network socket programming techniques, providing a basic yet robust platform for communication between a server and multiple clients over a local network. The combination of TCP/IP for reliable data transfer and UDP for server discovery will ensure seamless connectivity.
