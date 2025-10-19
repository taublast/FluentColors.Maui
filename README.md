# FluentColors.Maui

[![NuGet](https://img.shields.io/nuget/v/FluentColors.Maui.svg)](https://www.nuget.org/packages/FluentColors.Maui/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Access colors declared inside .NET MAUI `Colors.xaml` 🎨 from code behind, with IntelliSense support.

## How It Works

1. **Add the NuGet package** to your MAUI project
2. **Source code generator runs** - a new file `AppColors.cs` is created
3. **Use your colors** with IntelliSense support from code-behind!

The source code generator finds `Resources/Colors.xaml`, parses all color definitions, and creates `Resources/AppColors.cs` with strongly-typed properties + fluent extensions!  
Runs only if `Colors.xaml` has changed.

**Before**  
```csharp
label.TextColor = App.Current.Resources["ColorAccent"] as Color; // No IntelliSense, possible runtime errors
```

**After**
```csharp
label.TextColor = AppColors.ColorAccent; // IntelliSense support, compile-time safety

// Bonus: fluent color extensions!
var colorPrimaryLightest = AppColors.Primary.Lighten(0.3f);
```

Customization project tags provided to change auto-generated class name etc.

---

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package FluentColors.Maui
```

Or add to your `.csproj`:

```xml
<PackageReference Include="FluentColors.Maui" Version="1.0.0.5" />
```

---

## Settings

You can additionally manage options by setting these tags inside your `.csproj`` file:

| Setting | Default | Description |
|---------|---------|-------------|
| `ColorResourcesGeneratorEnabled` | `true` | Enable/disable the generator |
| `ColorResourcesClassName` | `AppColors` | Name of the generated class |
| `ColorResourcesNamespace` | `$(RootNamespace)` | Namespace for the generated class |

 ---

## Fluent Extension Methods

The package includes powerful fluent extension methods for color manipulation:

### Opacity & Alpha

```csharp
// Set opacity as percentage (0-100)
var semiTransparent = AppColors.Primary.WithOpacity(50); // 50% opacity

// Set alpha as float (0.0-1.0)
var transparent = AppColors.Secondary.WithAlpha(0.75f); // 75% alpha
```

### Lighten & Darken

```csharp
// Create lighter version (default factor: 0.2)
var lighter = AppColors.Primary.Lighten(); // 20% lighter
var muchLighter = AppColors.Primary.Lighten(0.5f); // 50% lighter

// Create darker version (default factor: 0.2)
var darker = AppColors.Primary.Darken(); // 20% darker
var muchDarker = AppColors.Primary.Darken(0.5f); // 50% darker
```

### Hex Conversion

```csharp
// Convert color to hex string
string hex = AppColors.Primary.ToHex(); // "#3D9BD7"

// Convert with alpha channel
string hexWithAlpha = AppColors.Primary.ToHexWithAlpha(); // "#3D9BD7FF"
```

### Create from Hex

```csharp
// Create color from hex string (with or without #)
var color1 = ColorExtensions.FromHex("#3D9BD7");
var color2 = ColorExtensions.FromHex("3D9BD7");

// Supports shorthand format
var color3 = ColorExtensions.FromHex("#F00"); // Same as #FF0000

// With alpha channel (AARRGGBB format)
var color4 = ColorExtensions.FromHex("#803D9BD7"); // 50% opacity

// RGBA format (RRGGBBAA)
var color5 = ColorExtensions.FromHexRGBA("#3D9BD780"); // 50% opacity
```

### Chaining Methods

All extension methods are chainable for fluent syntax:

```csharp
// Chain multiple transformations
var customColor = AppColors.Primary
    .Lighten(0.2f)
    .WithOpacity(80);

// Complex transformations
var accentVariant = AppColors.ColorAccent
    .Darken(0.1f)
    .WithAlpha(0.9f);

// Use in UI assignments
myLabel.TextColor = AppColors.Gray500.Lighten(0.3f);
myButton.BackgroundColor = AppColors.Primary.WithOpacity(85);
```

### Practical Examples

```csharp
// Hover effect
buttonNormal.BackgroundColor = AppColors.Primary;
buttonHover.BackgroundColor = AppColors.Primary.Lighten(0.15f);

// Disabled state
buttonDisabled.BackgroundColor = AppColors.Primary.WithOpacity(40);

// Shadow effect
shadow.Color = AppColors.Black.WithAlpha(0.3f);

// Theme variations
var lightThemeBg = AppColors.Gray100.Lighten(0.5f);
var darkThemeBg = AppColors.Gray900.Darken(0.2f);

// Gradient colors
var gradientStart = AppColors.Primary;
var gradientEnd = AppColors.Primary.Darken(0.3f).WithOpacity(80);
```

---

## Troubleshooting

### Colors.xaml not found
**Issue**: Build warning "Colors.xaml not found at: ..."
**Solution**: Ensure `Resources/Colors.xaml` exists in your project

### Generated class not found
**Issue**: "The type or namespace name 'AppColors' could not be found"
**Solution**:
1. Build the project (generator runs on build)
2. Check that `Resources/AppColors.cs` was created
3. Verify the namespace matches your `RootNamespace`

### IntelliSense not showing colors
**Issue**: Can't see color properties in autocomplete
**Solution**:
1. Rebuild the project
2. Restart Visual Studio / IDE
3. Check the generated file exists

### Build errors after removing colors
**Issue**: Compilation errors for removed colors
**Solution**: This is expected! The generator provides compile-time safety. Update your code to remove references to deleted colors.

---

## Contributing

Issues and pull requests welcome! Visit our [GitHub repository](https://github.com/taublast/FluentColors.Maui).

---

**Enjoy strongly-typed, fluent color resources! 🎨**
