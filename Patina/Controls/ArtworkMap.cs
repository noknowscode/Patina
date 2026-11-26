using Mapsui;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI.WinUI;
using Patina.Core.Model;
using Patina.Core.Queries;
using SkiaSharp;

namespace Patina.Controls;

/// <summary>
/// The collection on a map: one pin per artwork, filled with its grade colour, the selected pin larger and ringed.
/// Tapping a pin sets <see cref="SelectedId"/>; tapping open map clears it.
///
/// Imperative by necessity: a map is a viewport, a tile cache and a hit test with no XAML surface. Everything the
/// page needs crosses through two bindable properties, so the page itself keeps no code-behind.
/// </summary>
public sealed partial class ArtworkMap : UserControl
{
    /// <summary>OpenStreetMap's tile policy requires an identifying User-Agent and blocks generic ones (runtime gotchas).</summary>
    private const string TileUserAgent = "Patina/1.0 (public art conservation field app; Uno Platform)";

    private const double TapTolerancePixels = 24;

    /// <summary>About 30 m per pixel: the whole sample collection fits a laptop window.</summary>
    private const double CityResolution = 30;

    private static readonly (double Lon, double Lat) Centre = (-73.585, 45.515);

    /// <summary>
    /// Typed <c>object</c> on purpose: MVUX projects an <c>IListFeed</c> as its own observable list type, so a
    /// strongly typed property would drop the binding. Any enumerable of <see cref="ArtworkSummary"/> works, and
    /// collection changes (search, filters, a submitted survey) redraw the pins.
    /// </summary>
    public static readonly DependencyProperty ArtworksProperty = DependencyProperty.Register(
        nameof(Artworks), typeof(object), typeof(ArtworkMap),
        new PropertyMetadata(null, (d, e) => ((ArtworkMap)d).OnArtworksChanged(e.OldValue, e.NewValue)));

    public static readonly DependencyProperty SelectedIdProperty = DependencyProperty.Register(
        nameof(SelectedId), typeof(string), typeof(ArtworkMap),
        new PropertyMetadata(null, (d, _) => ((ArtworkMap)d).Redraw()));

    private readonly Mapsui.UI.WinUI.MapControl _map = new();
    private MemoryLayer? _pins;
    private bool _initialized;
    private ElementTheme _tileTheme;
    private PinPalette _palette;

    public ArtworkMap()
    {
        Content = _map;
        _palette = PinPalette.For(ActualTheme);
        Loaded += (_, _) => Initialize();
        ActualThemeChanged += (_, _) =>
        {
            _palette = PinPalette.For(ActualTheme);
            ApplyTiles();
            Redraw();
        };
    }

    public object? Artworks
    {
        get => GetValue(ArtworksProperty);
        set => SetValue(ArtworksProperty, value);
    }

    private IEnumerable<ArtworkSummary> Items =>
        Artworks is System.Collections.IEnumerable items ? items.OfType<ArtworkSummary>() : [];

    private void OnArtworksChanged(object? oldValue, object? newValue)
    {
        if (oldValue is System.Collections.Specialized.INotifyCollectionChanged oldList)
        {
            oldList.CollectionChanged -= OnItemsChanged;
        }

        if (newValue is System.Collections.Specialized.INotifyCollectionChanged newList)
        {
            newList.CollectionChanged += OnItemsChanged;
        }

        Redraw();
    }

    private void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Redraw();

    public string? SelectedId
    {
        get => (string?)GetValue(SelectedIdProperty);
        set => SetValue(SelectedIdProperty, value);
    }

    private void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        var map = new Mapsui.Map { CRS = "EPSG:3857" };
        _pins = new MemoryLayer { Name = "artworks", Features = [] };
        map.Layers.Add(_pins);

        // Assign the map first: the navigator only answers once the control has a size.
        _map.Map = map;
        _map.Info += OnInfo;
        ApplyTiles();

        var (x, y) = SphericalMercator.FromLonLat(Centre.Lon, Centre.Lat);
        map.Navigator.CenterOnAndZoomTo(new MPoint(x, y), CityResolution);
        Redraw();
    }

    private void ApplyTiles()
    {
        if (_map.Map is not { } map || (_tileTheme == ActualTheme && map.Layers.Any(l => l is TileLayer)))
        {
            return;
        }

        _tileTheme = ActualTheme;
        foreach (var old in map.Layers.OfType<TileLayer>().ToList())
        {
            map.Layers.Remove(old);
        }

        var osm = OpenStreetMap.CreateTileLayer(TileUserAgent);
        var ground = ThemeColors.Get("BackgroundColor", ActualTheme);
        var groundColor = new SKColor(ground.R, ground.G, ground.B);
        var matrix = ActualTheme == ElementTheme.Dark
            ? MutedTileSource.NightMatrix(groundColor, 0.06f)
            : MutedTileSource.GroundMatrix(groundColor, 0.36f, 0.66f);

        // The ground shows until tiles arrive (and stays when the tile server is unreachable).
        map.BackColor = Mapsui.Styles.Color.FromArgb(ground.A, ground.R, ground.G, ground.B);

        if (osm.TileSource is BruTile.IHttpTileSource http)
        {
            map.Layers.Insert(0, new TileLayer(new MutedTileSource(http, matrix)) { Name = "tiles" });
        }
        else
        {
            map.Layers.Insert(0, osm);
        }

        _map.Refresh();
    }

    private void Redraw()
    {
        if (_pins is null)
        {
            return;
        }

        var selected = SelectedId;
        _pins.Features = Items
            .OrderBy(a => a.Id == selected)
            .Select(a => Pin(a, a.Id == selected, _palette))
            .ToList();
        _map.Refresh();
    }

    private static PointFeature Pin(ArtworkSummary artwork, bool selected, PinPalette palette)
    {
        var (x, y) = SphericalMercator.FromLonLat(artwork.Longitude, artwork.Latitude);
        var feature = new PointFeature(x, y);
        feature["id"] = artwork.Id;
        feature.Styles.Add(new SymbolStyle
        {
            SymbolType = SymbolType.Ellipse,
            SymbolScale = selected ? 1.25 : 0.75,
            Fill = palette.Fill(artwork.Grade),
            Outline = selected ? palette.SelectedRing : palette.Ring,
        });
        return feature;
    }

    /// <summary>
    /// Pin fills and rings from Styles/Condition.xaml (Grade*InvariantColor, GradeUnsurveyedColor, MapPin*RingColor),
    /// resolved once per theme so a redraw only allocates the features.
    /// </summary>
    private readonly record struct PinPalette(Mapsui.Styles.Brush[] Fills, Pen Ring, Pen SelectedRing)
    {
        public static PinPalette For(ElementTheme theme)
        {
            Mapsui.Styles.Color Get(string key)
            {
                var c = ThemeColors.Get(key, theme);
                return Mapsui.Styles.Color.FromArgb(c.A, c.R, c.G, c.B);
            }

            return new PinPalette(
                [
                    new(Get("GradeUnsurveyedColor")),
                    new(Get("Grade1InvariantColor")),
                    new(Get("Grade2InvariantColor")),
                    new(Get("Grade3InvariantColor")),
                    new(Get("Grade4InvariantColor")),
                ],
                new Pen(Get("MapPinRingColor"), 2),
                new Pen(Get("MapPinSelectedRingColor"), 3));
        }

        public Mapsui.Styles.Brush Fill(ConditionGrade grade) => Fills[Math.Clamp((int)grade, 0, Fills.Length - 1)];
    }

    private void OnInfo(object? sender, MapInfoEventArgs e)
    {
        if (_map.Map?.Navigator.Viewport is not { } viewport)
        {
            return;
        }

        // Nearest pin within a fixed pixel tolerance, so the tap target is the same size at every zoom.
        var world = e.WorldPosition;
        ArtworkSummary? hit = null;
        var best = double.MaxValue;
        foreach (var artwork in Items)
        {
            var (x, y) = SphericalMercator.FromLonLat(artwork.Longitude, artwork.Latitude);
            var pixels = Math.Sqrt(((x - world.X) * (x - world.X)) + ((y - world.Y) * (y - world.Y))) / viewport.Resolution;
            if (pixels < best)
            {
                best = pixels;
                hit = artwork;
            }
        }

        SelectedId = best <= TapTolerancePixels ? hit?.Id : null;
        e.Handled = true;
    }
}
