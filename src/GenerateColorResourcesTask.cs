using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FluentColors.Maui
{
    /// <summary>
    /// MSBuild task that generates strongly-typed C# helper class from MAUI Colors.xaml
    /// </summary>
    public class GenerateColorResourcesTask : Task
    {
        /// <summary>
        /// Path to the Colors.xaml file to parse
        /// </summary>
        [Required]
        public string? ColorsXamlPath { get; set; }

        /// <summary>
        /// Path where the generated ResourcesColors.cs file should be written
        /// </summary>
        [Required]
        public string? OutputPath { get; set; }

        /// <summary>
        /// Namespace for the generated class (defaults to project root namespace)
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// Name of the generated class 
        /// </summary>
        public string ClassName { get; set; } = "AppColors";

        public override bool Execute()
        {
            try
            {
                if (string.IsNullOrEmpty(ColorsXamlPath))
                {
                    Log.LogError("ColorsXamlPath is required");
                    return false;
                }

                if (string.IsNullOrEmpty(OutputPath))
                {
                    Log.LogError("OutputPath is required");
                    return false;
                }

                if (!File.Exists(ColorsXamlPath))
                {
                    Log.LogWarning($"Colors.xaml not found at: {ColorsXamlPath}. Skipping code generation.");
                    return true; // Not an error, just skip
                }

                Log.LogMessage(MessageImportance.High, $"Generating {ClassName}.cs from Colors.xaml...");
                Log.LogMessage(MessageImportance.Normal, $"  Input:  {ColorsXamlPath}");
                Log.LogMessage(MessageImportance.Normal, $"  Output: {OutputPath}");

                // Parse XAML
                var colorResources = ParseColorResources(ColorsXamlPath);

                if (colorResources.Count == 0)
                {
                    Log.LogWarning($"No color resources found in {ColorsXamlPath}");
                    return true;
                }

                Log.LogMessage(MessageImportance.Normal, $"  Found {colorResources.Count} color resources");

                // Determine namespace
                string targetNamespace = Namespace;
                if (string.IsNullOrEmpty(targetNamespace))
                {
                    // Try to infer from directory structure
                    var outputDir = Path.GetDirectoryName(OutputPath);
                    var resourcesDir = Path.GetDirectoryName(ColorsXamlPath);

                    // Default fallback
                    targetNamespace = "Resources";
                }

                // Generate code
                string generatedCode = GenerateClass(colorResources, targetNamespace, ClassName);

                // Only write if content changed (avoid triggering hot reload unnecessarily)
                bool shouldWrite = true;
                if (File.Exists(OutputPath))
                {
                    string existingContent = File.ReadAllText(OutputPath);

                    // Compare without the timestamp line to avoid unnecessary regeneration
                    string existingWithoutTimestamp = System.Text.RegularExpressions.Regex.Replace(
                        existingContent,
                        @"// Generated on: .*\r?\n",
                        "");
                    string newWithoutTimestamp = System.Text.RegularExpressions.Regex.Replace(
                        generatedCode,
                        @"// Generated on: .*\r?\n",
                        "");

                    shouldWrite = existingWithoutTimestamp != newWithoutTimestamp;

                    if (!shouldWrite)
                    {
                        Log.LogMessage(MessageImportance.Normal, $"⚡ {ClassName}.cs is up-to-date, skipping generation");
                    }
                }

                if (shouldWrite)
                {
                    // Write to file
                    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
                    File.WriteAllText(OutputPath, generatedCode);
                    Log.LogMessage(MessageImportance.High, $"✓ Successfully generated {ClassName}.cs with {colorResources.Count} colors");
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error generating color resources: {ex.Message}");
                Log.LogMessage(MessageImportance.Low, ex.StackTrace);
                return false;
            }
        }

        private Dictionary<string, string> ParseColorResources(string xamlPath)
        {
            var colors = new Dictionary<string, string>();

            try
            {
                XDocument doc = XDocument.Load(xamlPath);
                XNamespace ns = "http://schemas.microsoft.com/dotnet/2021/maui";
                XNamespace x = "http://schemas.microsoft.com/winfx/2009/xaml";

                // Find all Color elements with x:Key attribute
                var colorElements = doc.Descendants(ns + "Color")
                    .Where(e => e.Attribute(x + "Key") != null);

                foreach (var element in colorElements)
                {
                    string? key = element.Attribute(x + "Key")?.Value;
                    string value = element.Value;

                    if (!string.IsNullOrEmpty(key))
                    {
                        colors[key] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse XAML: {ex.Message}", ex);
            }

            return colors;
        }

        private string GenerateClass(Dictionary<string, string> colors, string namespaceName, string className)
        {
            var sb = new StringBuilder();

            // File header
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// This code was generated by Maui.ColorResourcesGenerator");
            sb.AppendLine($"// Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine("// Do not edit this file manually - changes will be overwritten!");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine("#nullable enable");
            sb.AppendLine();

            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine("{");

            // Generate custom exception class
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Exception thrown when a color resource cannot be found in the application resources.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public class ColorResourceNotFoundException : System.Exception");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Initializes a new instance of ColorResourceNotFoundException.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"colorKey\">The key of the color resource that was not found.</param>");
            sb.AppendLine("        public ColorResourceNotFoundException(string colorKey)");
            sb.AppendLine("            : base($\"Color resource '{colorKey}' not found in application resources. Ensure Colors.xaml is properly loaded.\")");
            sb.AppendLine("        {");
            sb.AppendLine("            ColorKey = colorKey;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Gets the key of the color resource that was not found.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public string ColorKey { get; }");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Auto-generated fluent helper class for accessing color resources from Colors.xaml");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine($"    public static class {className}");
            sb.AppendLine("    {");

            // Group colors by category
            var standardColors = colors.Where(c => !c.Key.StartsWith("Color") &&
                                                   !c.Key.Contains("Accent") &&
                                                   !c.Key.StartsWith("Gray")).ToList();
            var grayColors = colors.Where(c => c.Key.StartsWith("Gray")).ToList();
            var accentColors = colors.Where(c => c.Key.Contains("Accent") && !c.Key.StartsWith("Color")).ToList();
            var colorPrefixed = colors.Where(c => c.Key.StartsWith("Color")).ToList();

            // Generate standard colors
            if (standardColors.Any())
            {
                sb.AppendLine("        #region Standard Colors");
                sb.AppendLine();
                foreach (var color in standardColors.OrderBy(c => c.Key))
                {
                    GenerateColorProperty(sb, color.Key, color.Value);
                }
                sb.AppendLine("        #endregion");
                sb.AppendLine();
            }

            // Generate gray scale colors
            if (grayColors.Any())
            {
                sb.AppendLine("        #region Gray Scale Colors");
                sb.AppendLine();
                foreach (var color in grayColors.OrderBy(c => c.Key))
                {
                    GenerateColorProperty(sb, color.Key, color.Value);
                }
                sb.AppendLine("        #endregion");
                sb.AppendLine();
            }

            // Generate accent colors
            if (accentColors.Any())
            {
                sb.AppendLine("        #region Accent Colors");
                sb.AppendLine();
                foreach (var color in accentColors.OrderBy(c => c.Key))
                {
                    GenerateColorProperty(sb, color.Key, color.Value);
                }
                sb.AppendLine("        #endregion");
                sb.AppendLine();
            }

            // Generate color-prefixed variants
            if (colorPrefixed.Any())
            {
                sb.AppendLine("        #region Custom Colors");
                sb.AppendLine();
                foreach (var color in colorPrefixed.OrderBy(c => c.Key))
                {
                    GenerateColorProperty(sb, color.Key, color.Value);
                }
                sb.AppendLine("        #endregion");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine();

            // Generate fluent color extension methods
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Fluent extension methods for working with colors");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class ColorExtensions");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a new color with the specified alpha/opacity value (0.0 to 1.0)");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color WithAlpha(this Microsoft.Maui.Graphics.Color color, float alpha)");
            sb.AppendLine("        {");
            sb.AppendLine("            return color.WithAlpha(System.Math.Clamp(alpha, 0f, 1f));");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a new color with the specified opacity percentage (0 to 100)");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color WithOpacity(this Microsoft.Maui.Graphics.Color color, int percentage)");
            sb.AppendLine("        {");
            sb.AppendLine("            return color.WithAlpha(System.Math.Clamp(percentage / 100f, 0f, 1f));");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a lighter version of the color");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"factor\">Amount to lighten (0.0 to 1.0, default 0.2)</param>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color Lighten(this Microsoft.Maui.Graphics.Color color, float factor = 0.2f)");
            sb.AppendLine("        {");
            sb.AppendLine("            factor = System.Math.Clamp(factor, 0f, 1f);");
            sb.AppendLine("            return new Microsoft.Maui.Graphics.Color(");
            sb.AppendLine("                color.Red + (1 - color.Red) * factor,");
            sb.AppendLine("                color.Green + (1 - color.Green) * factor,");
            sb.AppendLine("                color.Blue + (1 - color.Blue) * factor,");
            sb.AppendLine("                color.Alpha);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a darker version of the color");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"factor\">Amount to darken (0.0 to 1.0, default 0.2)</param>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color Darken(this Microsoft.Maui.Graphics.Color color, float factor = 0.2f)");
            sb.AppendLine("        {");
            sb.AppendLine("            factor = System.Math.Clamp(factor, 0f, 1f);");
            sb.AppendLine("            return new Microsoft.Maui.Graphics.Color(");
            sb.AppendLine("                color.Red * (1 - factor),");
            sb.AppendLine("                color.Green * (1 - factor),");
            sb.AppendLine("                color.Blue * (1 - factor),");
            sb.AppendLine("                color.Alpha);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts the color to a hex string (e.g., #FF5733)");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static string ToHex(this Microsoft.Maui.Graphics.Color color)");
            sb.AppendLine("        {");
            sb.AppendLine("            return $\"#{(int)(color.Red * 255):X2}{(int)(color.Green * 255):X2}{(int)(color.Blue * 255):X2}\";");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Converts the color to a hex string with alpha (e.g., #FF5733AA)");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static string ToHexWithAlpha(this Microsoft.Maui.Graphics.Color color)");
            sb.AppendLine("        {");
            sb.AppendLine("            return $\"#{(int)(color.Red * 255):X2}{(int)(color.Green * 255):X2}{(int)(color.Blue * 255):X2}{(int)(color.Alpha * 255):X2}\";");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a Color from a hex string. Supports formats: #RGB, #RRGGBB, #AARRGGBB, RGB, RRGGBB, AARRGGBB");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"hex\">Hex color string with or without #</param>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color FromHex(string hex)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(hex))");
            sb.AppendLine("                throw new System.ArgumentException(\"Hex string cannot be null or empty\", nameof(hex));");
            sb.AppendLine();
            sb.AppendLine("            hex = hex.Trim().TrimStart('#');");
            sb.AppendLine();
            sb.AppendLine("            // Handle 3-digit shorthand (e.g., \"RGB\" -> \"RRGGBB\")");
            sb.AppendLine("            if (hex.Length == 3)");
            sb.AppendLine("                hex = $\"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}\";");
            sb.AppendLine();
            sb.AppendLine("            int alpha = 255;");
            sb.AppendLine("            int red, green, blue;");
            sb.AppendLine();
            sb.AppendLine("            if (hex.Length == 6)");
            sb.AppendLine("            {");
            sb.AppendLine("                // RRGGBB");
            sb.AppendLine("                red = System.Convert.ToInt32(hex.Substring(0, 2), 16);");
            sb.AppendLine("                green = System.Convert.ToInt32(hex.Substring(2, 2), 16);");
            sb.AppendLine("                blue = System.Convert.ToInt32(hex.Substring(4, 2), 16);");
            sb.AppendLine("            }");
            sb.AppendLine("            else if (hex.Length == 8)");
            sb.AppendLine("            {");
            sb.AppendLine("                // AARRGGBB");
            sb.AppendLine("                alpha = System.Convert.ToInt32(hex.Substring(0, 2), 16);");
            sb.AppendLine("                red = System.Convert.ToInt32(hex.Substring(2, 2), 16);");
            sb.AppendLine("                green = System.Convert.ToInt32(hex.Substring(4, 2), 16);");
            sb.AppendLine("                blue = System.Convert.ToInt32(hex.Substring(6, 2), 16);");
            sb.AppendLine("            }");
            sb.AppendLine("            else");
            sb.AppendLine("            {");
            sb.AppendLine("                throw new System.ArgumentException($\"Invalid hex color format: {hex}. Expected 3, 6, or 8 characters.\", nameof(hex));");
            sb.AppendLine("            }");
            sb.AppendLine();
            sb.AppendLine("            return new Microsoft.Maui.Graphics.Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Creates a Color from RGBA hex string. Supports formats: #RRGGBBAA, RRGGBBAA");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"hex\">RGBA hex color string with or without #</param>");
            sb.AppendLine("        public static Microsoft.Maui.Graphics.Color FromHexRGBA(string hex)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (string.IsNullOrWhiteSpace(hex))");
            sb.AppendLine("                throw new System.ArgumentException(\"Hex string cannot be null or empty\", nameof(hex));");
            sb.AppendLine();
            sb.AppendLine("            hex = hex.Trim().TrimStart('#');");
            sb.AppendLine();
            sb.AppendLine("            if (hex.Length != 8)");
            sb.AppendLine("                throw new System.ArgumentException($\"Invalid RGBA hex format: {hex}. Expected 8 characters (RRGGBBAA).\", nameof(hex));");
            sb.AppendLine();
            sb.AppendLine("            int red = System.Convert.ToInt32(hex.Substring(0, 2), 16);");
            sb.AppendLine("            int green = System.Convert.ToInt32(hex.Substring(2, 2), 16);");
            sb.AppendLine("            int blue = System.Convert.ToInt32(hex.Substring(4, 2), 16);");
            sb.AppendLine("            int alpha = System.Convert.ToInt32(hex.Substring(6, 2), 16);");
            sb.AppendLine();
            sb.AppendLine("            return new Microsoft.Maui.Graphics.Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void GenerateColorProperty(StringBuilder sb, string key, string hexValue)
        {
            // Sanitize property name: replace hyphens with underscores for valid C# identifiers
            string propertyName = key.Replace("-", "_");

            // Convert property name to camelCase for private field
            string fieldName = "_" + char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);

            sb.AppendLine($"        private static Microsoft.Maui.Graphics.Color? {fieldName};");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine($"        /// Color resource: {key} ({hexValue})");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine($"        public static Microsoft.Maui.Graphics.Color {propertyName}");
            sb.AppendLine("        {");
            sb.AppendLine("            get");
            sb.AppendLine("            {");
            sb.AppendLine($"                if ({fieldName} == null)");
            sb.AppendLine("                {");
            sb.AppendLine($"                    {fieldName} = Microsoft.Maui.Controls.Application.Current?.Resources[\"{key}\"] as Microsoft.Maui.Graphics.Color");
            sb.AppendLine($"                        ?? throw new ColorResourceNotFoundException(\"{key}\");");
            sb.AppendLine("                }");
            sb.AppendLine($"                return {fieldName};");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();
        }
    }
}
