namespace Ryo.Reloaded.CRI.CriAtom.Models;

public sealed record CategoryInfo(string Name, uint Id, ushort Index, ushort GroupIndex);
public sealed record GlobalAisacInfo(string Name, ushort Index, ushort StoredControlId);
public sealed record AisacControlInfo(string Name, ushort StoredId, ushort MapIndex)
{
    public ushort PublicId => StoredId >= 1000 ? checked((ushort)(StoredId - 1000))
        : throw new NotSupportedException("Expected the observed ACF control-ID encoding (1000 + public ID).");
}

/// <summary>Reads the separate name/index tables of the supplied ACF 1.20 schema.</summary>
public sealed class AcfCatalog
{
    public IReadOnlyList<CategoryInfo> Categories { get; }
    public IReadOnlyList<GlobalAisacInfo> GlobalAisacs { get; }
    public IReadOnlyList<AisacControlInfo> Controls { get; }
    public uint CategoriesPerPlayback { get; }
    public IReadOnlyList<string> BusNames { get; }
    public string Name { get; }
    public AcfCatalog(byte[] bytes)
    {
        var root = UtfTable.Read(bytes);
        if (root.Rows.Count != 1) throw new InvalidDataException("Expected one ACF header row.");
        Name = root.Text(0, "Name");
        CategoriesPerPlayback = root.Has("CategoriesParPlayback") ? root.Number(0, "CategoriesParPlayback") : 4;
        UtfTable Table(string name) => UtfTable.Read(root.Data(0, name));
        var cats = Table("CategoryTable"); var names = Table("CategoryNameTable");
        var categories = new List<CategoryInfo>();
        for (int i = 0; i < names.Rows.Count; i++)
        {
            ushort index = checked((ushort)names.Number(i, "Index"));
            if (index >= cats.Rows.Count) throw new InvalidDataException("Invalid category name-table index.");
            categories.Add(new(names.Text(i, "Name"), cats.Number(index, "Id"), index,
                checked((ushort)cats.Number(index, "GroupIndex"))));
        }
        if (categories.Select(c => c.Name).Distinct(StringComparer.Ordinal).Count() != categories.Count ||
            categories.Select(c => c.Id).Distinct().Count() != categories.Count)
            throw new InvalidDataException("Ambiguous ACF category names or IDs.");
        Categories = categories.AsReadOnly();
        var global = new List<GlobalAisacInfo>();
        if (root.Has("GlobalAisacNameTable") && root.Data(0, "GlobalAisacNameTable").Length > 0)
        {
            var gn = Table("GlobalAisacNameTable"); var gt = Table("GlobalAisacTable");
            for (int i = 0; i < gn.Rows.Count; i++)
            {
                ushort index = checked((ushort)gn.Number(i, "Index"));
                if (index >= gt.Rows.Count) throw new InvalidDataException("Invalid global AISAC index.");
                global.Add(new(gn.Text(i, "Name"), index, checked((ushort)gt.Number(index, "ControlId"))));
            }
        }
        GlobalAisacs = global.AsReadOnly();
        var controls = new List<AisacControlInfo>();
        string controlTable = root.Has("AisacControlIdTable") ? "AisacControlIdTable" : "AisacControlNameTable";
        var ct = Table(controlTable);
        for (int i = 0; i < ct.Rows.Count; i++)
            controls.Add(new(ct.Text(i, "AisacControlName"), checked((ushort)ct.Number(i, "AisacControlId")), checked((ushort)i)));
        Controls = controls.AsReadOnly();
        var buses = root.Has("BusNameTable") ? Table("BusNameTable") : null;
        BusNames = buses is null ? [] : Enumerable.Range(0, buses.Rows.Count).Select(i => buses.Text(i, "StringValue")).ToArray();
    }
    public CategoryInfo Category(string name) => Categories.SingleOrDefault(c => c.Name == name)
        ?? throw new ArgumentException($"Unknown ACF category '{name}'.");
    public CategoryInfo Category(uint id) => Categories.SingleOrDefault(c => c.Id == id)
        ?? throw new ArgumentException($"Unknown ACF category ID {id}.");
    public GlobalAisacInfo GlobalAisac(string name) => GlobalAisacs.SingleOrDefault(c => c.Name == name)
        ?? throw new ArgumentException($"Unknown global AISAC '{name}'.");
}
