# FluentColors.Maui

[![NuGet](https://img.shields.io/nuget/v/FluentColors.Maui.svg)](https://www.nuget.org/packages/FluentColors.Maui/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Access colors declared inside .NET MAUI `Colors.xaml` 🎨 from code behind, with IntelliSense support.

## How It Works

1. **Add the NuGet package** to your MAUI project
2. **Build your project** - the generator runs automatically
3. **Use your colors** with IntelliSense support from code-behind!

The source code generator finds `Resources/Colors.xaml`, parses all color definitions, and creates `Resources/AppColors.cs` with strongly-typed properties + fluent extensions!

**Before**  
```csharp
label.TextColor = App.Current.Resources["ColorAccent"] as Color; // No IntelliSense, possible runtime errors
```

**After** 
```csharp

label.TextColor = AppColors.ColorAccent; // IntelliSense support

// Bonus: additional color extensions!
var colorAccentLigther = AppColors.Primary.Lighten(0.3f);
```

Customization project tags provided to change auto-generated class name etc.

## Installation

Install via NuGet Package Manager:

```bash
dotnet add package FluentColors.Maui
```

Or add to your `.csproj`:

```xml
<PackageReference Include="FluentColors.Maui" Version="1.0.0.5" />
```


### Available Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `ColorResourcesGeneratorEnabled` | `true` | Enable/disable the generator |
| `ColorResourcesClassName` | `AppColors` | Name of the generated class |
| `ColorResourcesNamespace` | `$(RootNamespace)` | Namespace for the generated class |

### Smart Incremental Build

The generator is smart about regeneration:
- ✅ **Only runs if Colors.xaml changed** - Won't trigger hot reload unnecessarily
- ✅ **Compares content, not timestamps** - Ignores timestamp-only changes
- ✅ **Fast builds** - Skips generation when nothing changed

You'll see:
- `✓ Successfully generated AppColors.cs` - File was regenerated
- `⚡ AppColors.cs is up-to-date, skipping generation` - Nothing changed, no conflict with HotReload


## Requirements

- .NET MAUI 8.0+
- `Resources/Colors.xaml` file in standard MAUI format

### Color Categories
Colors are automatically grouped into regions:
- **Standard Colors**: Basic MAUI colors (Primary, Secondary, Tertiary, Black, White)
- **Gray Scale Colors**: Colors starting with "Gray" (Gray100, Gray200, etc.)
- **Accent Colors**: Colors containing "Accent" (Blue100Accent, Yellow200Accent, etc.)
- **Custom Colors**: All other colors (ColorAccent, ColorSuccess, etc.)

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

## Contributing

Issues and pull requests welcome! Visit our [GitHub repository](https://github.com/taublast/FluentColors.Maui).

## License

MIT License - see LICENSE file for details.

## Credits

Created by Nick Kovalsky aka AppoMobi for the MAUI community.

---

**Enjoy strongly-typed, fluent color resources! 🎨**
