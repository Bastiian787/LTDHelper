using Geode.Network.Protocol;

namespace LTDHelper;

/// <summary>
/// Parses a Habbo catalog index node from a G-Earth intercepted packet.
/// Replaces the <c>HCatalogNode</c> class that was removed in G-Earth-Geode 1.4.1-beta.
/// </summary>
public class HCatalogNode
{
    public bool IsVisible { get; }
    public int Icon { get; }
    public int PageId { get; }
    public string PageName { get; }
    public int[] OfferIds { get; }
    public HCatalogNode[] Children { get; }

    /// <summary>
    /// Parses a single catalog node from the current position in <paramref name="packet"/>.
    /// The caller is responsible for advancing the position to the start of the node.
    /// </summary>
    public HCatalogNode(HPacket packet)
    {
        IsVisible = packet.ReadBoolean();
        Icon = packet.ReadInt32();
        PageId = packet.ReadInt32();
        PageName = packet.ReadUTF8();

        int offerCount = packet.ReadInt32();
        OfferIds = new int[offerCount];
        for (int i = 0; i < offerCount; i++)
            OfferIds[i] = packet.ReadInt32();

        int childCount = packet.ReadInt32();
        Children = new HCatalogNode[childCount];
        for (int i = 0; i < childCount; i++)
            Children[i] = new HCatalogNode(packet);
    }

    /// <summary>
    /// Parses the root catalog node from a full <c>CatalogIndex</c> packet
    /// (position 0). The leading catalog-type string is consumed automatically.
    /// </summary>
    public static HCatalogNode FromCatalogIndexPacket(HPacket packet)
    {
        packet.ReadUTF8(); // Skip catalog type (e.g., "NORMAL")
        return new HCatalogNode(packet);
    }
}
