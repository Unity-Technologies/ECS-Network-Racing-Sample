# Race Debug Logging System

A comprehensive debug and logging system for the ECS Network Racing Sample that provides:

- Color-coded console logs for different types of events
- Tracking of game state transitions
- Logging of player actions
- Network event logging
- Easy integration with existing systems

## 🚀 Getting Started

1. Add the `RaceLoggerSetup` component to a GameObject in your main scene
2. Create a `RaceLoggerSettings` asset by right-clicking in the Project view and selecting `Create > Racing > Debug > Logger Settings`
3. Assign the settings asset to the `RaceLoggerSetup` component

## 📝 Log Types

The logging system supports different types of logs with distinct colors:

- **Info (Cyan)**: General information messages
- **Success (Green)**: Successful operations and achievements
- **Warning (Yellow)**: Potential issues or noteworthy events
- **Error (Red)**: Error conditions
- **Gameplay (Magenta)**: Game state and gameplay events
- **Network (Blue)**: Networking and connection events
- **Physics (Teal)**: Physics-related events

## 🧰 Usage Examples

### Basic Logging

```csharp
// Log an informational message (cyan)
RaceLogger.Info("Game initialized");

// Log a success message (green)
RaceLogger.Success("Player connected successfully");

// Log a warning (yellow)
RaceLogger.Warning("Performance may be affected");

// Log an error (red)
RaceLogger.Error("Failed to load asset");

// Log a gameplay event (magenta)
RaceLogger.Gameplay("Player entered finish line");

// Log a network event (blue)
RaceLogger.Network("Connected to server: 192.168.1.1");

// Log a physics event (teal)
RaceLogger.Physics("Collision detected");
```

### Log Sections

To group related logs, you can create labeled sections:

```csharp
RaceLogger.LogSection("RACE STARTED");
```

### Custom Colored Logs

You can use custom colors by specifying a hex color:

```csharp
RaceLogger.CustomColor("Special event occurred", "#FF00FF");
```

## 🔧 Configuration

The `RaceLoggerSettings` asset allows you to configure:

- Enable/disable all logging
- Control which categories of logs are shown
- Configure advanced features like stack traces for errors

## 📚 Architecture

### Core Components

1. **RaceLogger**: Static utility class with methods for different types of logs
2. **RaceLoggerManager**: Singleton manager that initializes the logging system
3. **RaceLoggerSettings**: ScriptableObject for configuration
4. **RaceStateLoggerSystem**: ECS system that tracks race state changes
5. **RaceLoggerSetup**: MonoBehaviour for easy setup in scenes

### Integration Points

The logging system integrates with:

- **Race State System**: Logs state transitions during races
- **Car Selection UI**: Logs player car selections
- **ServerConnectionUtils**: Logs network connections and errors

## 📊 Additional Features

- Automatic initialization logging with system info
- Detailed state transition tracking
- Network error logging

## ⚙️ Customization

You can extend the system by:

1. Adding new log types to `RaceLogger`
2. Creating new integration points with other systems
3. Modifying the `RaceLoggerSettings` to include additional options 