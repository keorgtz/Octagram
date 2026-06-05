# MeridianUI · Platform Guide — WPF / XAML

## Setup

1. Agregar `tokens-wpf.xaml` al proyecto (ej. `Resources/MeridianTokens.xaml`)
2. Mergearlo en `App.xaml`:
   ```xml
   <Application.Resources>
       <ResourceDictionary>
           <ResourceDictionary.MergedDictionaries>
               <ResourceDictionary Source="Resources/MeridianTokens.xaml"/>
               <!-- MaterialDesignThemes.Wpf a continuación -->
               <ResourceDictionary Source="pack://application:,,,/MaterialDesignThemes.Wpf;component/Themes/MaterialDesign2.Defaults.xaml"/>
           </ResourceDictionary.MergedDictionaries>
       </ResourceDictionary>
   </Application.Resources>
   ```
3. Referenciar con `{StaticResource NombreToken}` en todos los controles.

## Naming convention — CSS → WPF key

| CSS variable    | WPF Key (Color)  | WPF Key (Brush)    |
|-----------------|------------------|--------------------|
| `--em`          | `EmColor`        | `EmBrush`          |
| `--em-md`       | `EmMdColor`      | `EmMdBrush`        |
| `--em-bg`       | `EmBgColor`      | `EmBgBrush`        |
| `--am`          | `AmColor`        | `AmBrush`          |
| `--in`          | `InColor`        | `InBrush`          |
| `--in-bg`       | `InBgColor`      | `InBgBrush`        |
| `--vi`          | `ViColor`        | `ViBrush`          |
| `--or`          | `OrColor`        | `OrBrush`          |
| `--primary`     | `PrimaryColor`   | `PrimaryBrush`     |
| `--gray-dark`   | `GrayDarkColor`  | `GrayDarkBrush`    |
| `--gray-muted`  | `GrayMutedColor` | `GrayMutedBrush`   |
| `--gray-line`   | `GrayLineColor`  | `GrayLineBrush`    |
| `--page-bg`     | `PageBgColor`    | `PageBgBrush`      |
| `--r-sm` (6px)  | —                | `RadiusSm`         |
| `--r-md` (10px) | —                | `RadiusMd`         |
| `--r-lg` (14px) | —                | `RadiusLg`         |
| `--r-xl` (20px) | —                | `RadiusXl`         |
| `--s1`…`--s8`  | —                | `S1`…`S8` (Double) |
| `--fs-13`       | —                | `Fs13` (Double)    |

## Example: KpiCard en XAML

```xml
<!-- KPI Card con accent bar y barra segmentada -->
<Border Style="{StaticResource KpiCard}" Width="210">
    <StackPanel>
        <!-- Accent bar superior (color según categoría) -->
        <Border Height="4" CornerRadius="20 20 0 0"
                Background="{StaticResource EmBrush}"
                Margin="-18 -14 -18 12"/>

        <!-- Label UPPERCASE -->
        <TextBlock Style="{StaticResource TLabel}"
                   Text="LLEGADAS"/>

        <!-- Número hero -->
        <TextBlock Style="{StaticResource TKpiValue}"
                   Text="{Binding Llegadas}"
                   Foreground="{StaticResource EmBrush}"
                   Margin="0 4 0 0"/>

        <!-- Badge % variación -->
        <Border Background="{StaticResource EmPlBrush}"
                CornerRadius="{StaticResource RadiusSm}"
                Padding="6 2" HorizontalAlignment="Left"
                Margin="0 4 0 8">
            <TextBlock Style="{StaticResource TBodySm}"
                       Foreground="{StaticResource EmBrush}"
                       Text="{Binding LlegadasPct, StringFormat='+{0:P0}'}"/>
        </Border>

        <!-- Barra segmentada: check-in / por llegar -->
        <Grid Height="6" Margin="0 4 0 0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="{Binding CheckInPct, Converter={StaticResource PctToStarConverter}}"/>
                <ColumnDefinition Width="{Binding PorLlegarPct, Converter={StaticResource PctToStarConverter}}"/>
            </Grid.ColumnDefinitions>
            <Border Grid.Column="0" Background="{StaticResource EmBrush}" CornerRadius="3 0 0 3"/>
            <Border Grid.Column="1" Background="{StaticResource EmLtBrush}" CornerRadius="0 3 3 0"/>
        </Grid>

        <!-- Leyenda de la barra -->
        <StackPanel Orientation="Horizontal" Margin="0 6 0 0">
            <Ellipse Width="6" Height="6" Fill="{StaticResource EmBrush}" Margin="0 0 4 0"/>
            <TextBlock Style="{StaticResource TBodySm}" Text="Check-in" Margin="0 0 8 0"/>
            <Ellipse Width="6" Height="6" Fill="{StaticResource EmLtBrush}" Margin="0 0 4 0"/>
            <TextBlock Style="{StaticResource TBodySm}" Text="Por llegar"/>
        </StackPanel>
    </StackPanel>
</Border>
```

## Example: Chip de pago

```xml
<Border Background="{StaticResource PayEfecBgBrush}"
        CornerRadius="{StaticResource RadiusMd}"
        Padding="{StaticResource PaddingChip}">
    <TextBlock Style="{StaticResource TBodySm}"
               Foreground="{StaticResource PayEfecBrush}"
               Text="Efectivo"/>
</Border>
```

## Example: Sidebar item activo vs hover

```xml
<!-- Item activo -->
<Border Background="{StaticResource SidebarActiveBgBrush}"
        CornerRadius="{StaticResource RadiusMd}"
        Margin="{StaticResource MarginSidebarItem}"
        Padding="{StaticResource PaddingSidebarItem}">
    <StackPanel Orientation="Horizontal">
        <!-- ícono Material Symbols via TextBlock con fuente TTF -->
        <TextBlock Text="&#xE871;" FontFamily="Material Symbols Rounded"
                   Foreground="{StaticResource SidebarActiveFgBrush}" FontSize="18"/>
        <TextBlock Text="Dashboard"
                   Foreground="{StaticResource SidebarActiveFgBrush}"
                   FontWeight="SemiBold" FontSize="{StaticResource Fs13}"
                   Margin="8 0 0 0"/>
    </StackPanel>
</Border>
```

## WPF-specific rules

- **Sin TextTransform:** WPF no tiene propiedad `TextTransform`. Para labels UPPERCASE, convertir en el ViewModel (`Text.ToUpper()`) o usar un `StringFormatConverter`.
- **Sin CharacterSpacing nativo:** Para letter-spacing en WPF, se puede usar una `InlineCollection` con `CharacterSpacing` vía código, o aceptar el espacio predeterminado.
- **DropShadowEffect tiene costo de render:** Para listas largas o grids densos, considerar usar un `Border` con `Margin` visual en lugar de shadow por elemento.
- **Tabular nums en importes:** En WPF usar `Typography.Variants="Tabular"` o cambiar a una fuente con tabular nums (Segoe UI los tiene).
- **Iconografía Material Symbols en WPF:** Instalar la fuente TTF (`MaterialSymbolsRounded-Regular.ttf`) como recurso de la app y referenciarla como `FontFamily`. Los unicode codepoints están en el catálogo de Google Fonts.
- **Transiciones:** Usar `Storyboard` con `DoubleAnimation` para opacity/width. Duración máxima 200ms, `EasingFunction: PowerEase Mode=EaseOut`.

## Colores MaterialDesignInXaml bridge

SHEndevour usa MDIX. Mapeo sugerido de brushes MDIX → MeridianUI:

| MDIX Key                    | MeridianUI equivalente |
|-----------------------------|------------------------|
| `PrimaryHueMidBrush`        | `PrimaryBrush`         |
| `PrimaryHueDarkBrush`       | `PrimaryDarkBrush`     |
| `MaterialDesignBackground`  | `PageBgBrush`          |
| `MaterialDesignCardBackground` | `WhiteBrush`        |
| `MaterialDesignBody`        | `Gray700Brush`         |
| `MaterialDesignBodyLight`   | `GrayMutedBrush`       |

No reemplazar los brushes de MDIX; agregar los de MeridianUI como recursos adicionales.
