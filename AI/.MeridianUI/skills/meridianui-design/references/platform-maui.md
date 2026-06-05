# MeridianUI · Platform Guide — .NET MAUI

## Setup

### 1. Fuentes en MauiProgram.cs

```csharp
builder
    .UseMauiApp<App>()
    .ConfigureFonts(fonts =>
    {
        // Descargar Inter de fonts.google.com y agregar a Resources/Fonts/
        fonts.AddFont("Inter-Regular.ttf",  "InterRegular");
        fonts.AddFont("Inter-Medium.ttf",   "InterMedium");
        fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
        fonts.AddFont("Inter-Bold.ttf",     "InterBold");
        // Material Symbols Rounded para iconografía
        fonts.AddFont("MaterialSymbolsRounded-Regular.ttf", "MaterialSymbols");
    });
```

### 2. Tokens en App.xaml

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/Styles/MeridianTokens.xaml"/>
            <!-- Estilos propios de la app a continuación -->
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 3. Página base

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    Style="{StaticResource MeridianPage}">
```

## Naming convention — CSS → MAUI key

| CSS variable  | MAUI Key (Color) |
|---------------|-----------------|
| `--em`        | `Em`            |
| `--em-bg`     | `EmBg`          |
| `--am`        | `Am`            |
| `--in`        | `In`            |
| `--in-bg`     | `InBg`          |
| `--vi`        | `Vi`            |
| `--or`        | `Or`            |
| `--primary`   | `Primary`       |
| `--gray-dark` | `GrayDark`      |
| `--page-bg`   | `PageBg`        |
| `--r-xl`      | `RadiusXl`      |
| `--s4`        | `S4`            |
| `--fs-13`     | `Fs13`          |

## Example: KpiCard en MAUI

```xml
<Border Style="{StaticResource KpiCard}">
    <Grid RowDefinitions="Auto,Auto,Auto,Auto">

        <!-- Label UPPERCASE -->
        <Label Style="{StaticResource TLabel}"
               Text="LLEGADAS"
               Grid.Row="0"/>

        <!-- Número hero -->
        <Label Style="{StaticResource TKpiValue}"
               Text="{Binding Llegadas}"
               TextColor="{StaticResource Em}"
               Grid.Row="1" Margin="0,4,0,0"/>

        <!-- Badge variación -->
        <Border BackgroundColor="{StaticResource EmPl}"
                StrokeShape="RoundRectangle 6"
                Stroke="Transparent" Padding="6,2"
                HorizontalOptions="Start" Margin="0,4,0,8"
                Grid.Row="2">
            <Label Style="{StaticResource TBodySm}"
                   TextColor="{StaticResource Em}"
                   Text="+12%"/>
        </Border>

        <!-- Barra segmentada -->
        <Grid Grid.Row="3" HeightRequest="6" Margin="0,4,0,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="7*"/>
                <ColumnDefinition Width="3*"/>
            </Grid.ColumnDefinitions>
            <BoxView Color="{StaticResource Em}" CornerRadius="3,0,0,3" Grid.Column="0"/>
            <BoxView Color="{StaticResource EmLt}" CornerRadius="0,3,3,0" Grid.Column="1"/>
        </Grid>
    </Grid>
</Border>
```

## Example: ícono Material Symbols

```xml
<!-- Cargar la fuente MaterialSymbols en MauiProgram y usar codepoints -->
<Label Text="&#xe871;"
       FontFamily="MaterialSymbols"
       FontSize="18"
       TextColor="{StaticResource GrayMuted}"/>
```

## Responsive layout en MAUI

```xml
<!-- Usar Grid con ColumnDefinitions * para KPIs responsivos -->
<Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto"
      ColumnSpacing="8" RowSpacing="8"
      Padding="{StaticResource PaddingCard}">
    <!-- 2 KPIs por fila en teléfono -->
</Grid>

<!-- En tablet (orientación landscape) usar 4 columnas -->
```

## MAUI-specific rules

- **TextTransform disponible:** MAUI sí tiene `TextTransform="Uppercase"` en `Label`. Úsarlo para todos los TLabel y TMeta.
- **CharacterSpacing disponible:** MAUI tiene `CharacterSpacing` en `Label`. Los tokens TLabel tienen CharacterSpacing=60, TMeta=80.
- **Shadow:** MAUI tiene `Shadow` como propiedad de View. Los estilos `CardBase` y `KpiCard` ya lo incluyen.
- **Stroke/StrokeShape:** Para Border con CornerRadius usar `StrokeShape="RoundRectangle {radius}"`.
- **CollectionView sobre ListView:** Para listas largas usar `CollectionView` con `VirtualizingLayout`.
- **Safe areas:** Usar `SafeAreaInsets` en iOS para el notch: `<Grid Margin="{OnPlatform iOS='0,44,0,34'}">`
- **Fonts:** siempre registrar en `MauiProgram.cs` antes de referenciar en XAML.
- **No hay Segoe UI en iOS/Android:** usar Inter. Solo en Windows (MAUI) está disponible Segoe UI.
