```
                   $$\     $$\                     
                   $$ |    \__|                    
 $$$$$$$\        $$$$$$\   $$\  $$$$$$\   $$$$$$\  
$$  _____|$$$$$$\\_$$  _|  $$ |$$  __$$\ $$  __$$\ 
$$ /      \______| $$ |    $$ |$$$$$$$$ |$$ |  \__|
$$ |               $$ |$$\ $$ |$$   ____|$$ |      
\$$$$$$$\          \$$$$  |$$ |\$$$$$$$\ $$ |      
 \_______|          \____/ \__| \_______|\__|      
                                               

                                                 
```
# c-tier                                                 
#### An IRC-based chatting platform written in C#.

c-tier is a lightweight yet powerful chat system inspired by traditional IRC (Internet Relay Chat). Built for modern use, it offers flexibility, speed, and customization, whether you need a personal chat server or a solution for community-driven discussions.

## 🚀 Features at a Glance
#### 1. CLI Client
The c-tier client is a terminal-based application that operates seamlessly in any CLI environment. This minimalistic approach provides maximum control and flexibility for users who prefer command-line interfaces.

#### 2. Standalone Server Distribution
c-tier includes a customizable server that can be tailored to fit your specific needs with basic knowledge of Visual Studio and C#. Built on .NET 8, the server is lightweight, robust, and easy to deploy.

#### 3. Custom Endpoints
c-tier's architecture relies on socket-to-socket communication, where data routing is managed by user-defined endpoints. These endpoints enable you to extend functionality effortlessly by routing data to your custom classes.
👉 Learn more about setting up endpoints in our documentation.

#### 4. Unified Socket Architecture
To ensure secure and efficient communication, c-tier uses a single socket on both client and server sides. This architecture minimizes complexity while maintaining optimal speed and reliability.

#### 5. Channel Management
Channels are the core of c-tier's communication system. Users can:
  Create or delete channels on the fly.
  Restrict access based on roles and permissions, ensuring conversations remain private or public as needed.
#### 6. Embedded SQLite3 Database
All server data is stored using an embedded SQLite3 database, which is updated dynamically. This approach ensures a lightweight footprint with persistent, reliable data storage.

#### 7. Cross-Platform Compatibility
Built on C#, c-tier runs natively on Windows, macOS, and Linux, ensuring seamless operation across all major platforms.

## 🎯 Why Choose c-tier?
Simplicity and Speed: The CLI-based client and streamlined server design prioritize performance without unnecessary overhead.
Customizable and Extensible: Leverage the power of C# and .NET to modify, enhance, or integrate c-tier into larger systems.
Lightweight but Powerful: With embedded SQLite3 and efficient socket communication, c-tier packs robust features in a small package.
Modern Take on IRC: Inspired by classic chat systems, c-tier brings modern design principles to an established concept.
## 🛠️ Getting Started
#### Requirements
  .NET 8 SDK: Ensure you have the latest .NET runtime installed.
  SQLite3 (optional for advanced users): Pre-installed for database management if you prefer external tooling.
## Installation

  1.Clone the repository:
  ```
  git clone https://github.com/yourusername/c-tier.git
  cd c-tier
  ```
  2.Build the project using Visual Studio or the .NET CLI:
  ```
  dotnet build
  ```
## Running c-tier
 Launch the CLI client/Server(s or c):
  ```
  dotnet run src/Program.cs
  ```
## 📚 Documentation
Comprehensive documentation is available here. You’ll find:

Step-by-step guides for setting up endpoints.
Tutorials on creating custom channels and managing permissions.
Advanced configuration options for the server.
## 👥 Community and Support
Issues & Feedback: Found a bug or have a feature request? Open an issue on GitHub Issues.
## 📜 License
c-tier is released under the MIT License. Feel free to use, modify, and distribute this software.

# ***Happy Chatting!***

