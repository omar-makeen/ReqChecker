# Application Icons and Branding Assets

## Icon Requirements

### Main Application Icon
- **File**: `app.ico`
- **Sizes**: 16x16, 32x32, 48x48, 256x256 pixels
- **Format**: ICO format with multiple resolutions embedded
- **Color**: Blue (#0078D4) primary with white/gray variants
- **Style**: Modern, flat design with subtle depth

### Design Guidelines

1. **Primary Color**: #0078D4 (Microsoft Blue)
2. **Secondary Colors**:
   - Light: #FFFFFF (White)
   - Gray: #666666 (Medium Gray)
   - Dark: #202020 (Dark Gray)

3. **Icon Concept**:
   - A checkmark (✓) symbolizing validation/testing
   - Simple, clean lines
   - Minimalist design for Windows 11 aesthetic

### Required Icon Files

| File Name | Purpose | Size |
|------------|---------|-------|
| app.ico | Application icon | Multi-resolution (16, 32, 48, 256) |
| app-16.png | Small icon | 16x16 |
| app-32.png | Normal icon | 32x32 |
| app-48.png | Large icon | 48x48 |
| app-256.png | Extra large | 256x256 |

### Icon Placement

The icon should be referenced in:
1. **ReqChecker.App.csproj** - `<ApplicationIcon>Resources\Icons\app.ico</ApplicationIcon>`
2. **MainWindow.xaml** - Window icon
3. **installer/ReqChecker.wxs** - `<Icon Id="AppIcon" SourceFile="..\src\ReqChecker.App\Resources\Icons\app.ico"/>`

### Implementation Notes

- Use an icon editor like GIMP, IcoFX, or online converters
- Start with a high-resolution PNG (256x256) and downsample
- Ensure proper transparency for non-square backgrounds
- Test icon visibility on both light and dark backgrounds
