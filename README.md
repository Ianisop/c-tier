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
#### A modern twist on IRC, built with C#.  

c-tier is a lightweight, flexible chat platform inspired by the simplicity of IRC. Whether you're setting up a private chat server or a community hub, c-tier is built to be fast, customizable, and easy to use.

## 🚀 Key Features  

### 1. **Command-Line Client**  
The c-tier client is a terminal-based application, designed for anyone who loves the power and simplicity of command-line interfaces. It’s straightforward, efficient, and works seamlessly in any CLI environment.

### 2. **Standalone Server Distribution**  
c-tier includes a ready-to-use server, easily customized with basic C# knowledge and Visual Studio. Built on .NET 8, the server is lightweight, reliable, and easy to deploy.

### 3. **Custom Endpoints**  
c-tier uses socket-to-socket communication, with routing handled by user-defined endpoints. These endpoints make it easy to extend functionality and integrate custom features.  
👉 Check out our documentation for details on setting up your own endpoints.

### 4. **Unified Socket Architecture**  
With a single socket handling both client and server communications, c-tier keeps things fast, secure, and simple. This streamlined architecture minimizes overhead without sacrificing performance.

### 5. **Channel Management**  
Channels are at the heart of c-tier's chat system. You can:  
  - Create and delete channels dynamically.  
  - Restrict access with roles and permissions, keeping conversations private or open as needed.

### 6. **Embedded SQLite3 Database**  
Server data is stored using an embedded SQLite3 database, ensuring lightweight yet reliable storage that updates dynamically as your server operates.

### 7. **Cross-Platform Compatibility**  
Built with C#, c-tier runs natively on Windows, macOS, and Linux, so you can deploy it wherever you need it.

---

## 🎯 Why Choose c-tier?  
- **Simple & Fast**: The CLI client and efficient server design focus on speed without unnecessary fluff.  
- **Customizable**: Modify, extend, or integrate c-tier into other systems using the flexibility of C# and .NET.  
- **Lightweight, Yet Powerful**: A compact package with robust features like embedded SQLite3 and efficient socket communication.  
- **Modern IRC**: It’s inspired by classic chat systems but updated with today’s technology in mind.

---

## 🛠️ Getting Started  

### Requirements  
- **.NET 8 SDK**: Make sure you have the latest .NET runtime installed.  
- **SQLite3 (optional)**: Pre-installed if you want to manage the database externally.  

### Installation  
1. Clone the repository:  
  ```
  git clone https://github.com/Ianisop/c-tier.git
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

