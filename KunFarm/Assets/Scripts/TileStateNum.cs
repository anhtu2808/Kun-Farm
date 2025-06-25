public enum TileState
{
    Undug,    // Đất chưa đào (mặc định)
    Dug,      // Đất đã đào
    Planted,  // Đất đã trồng cây
    Harvested // Đất đã thu hoạch (có thể chuyển về Dug hoặc Undug)
}