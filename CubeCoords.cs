public struct TripleTuple<T> {
    public T x, y, z;

    public T this[int dim] {
        get => dim switch {
            0 => x, 1 => y, _ => z
        };
        set {
            if (dim == 0) x = value;
            else if (dim == 1) y = value;
            else z = value;
        }
    } 
}
public record struct CubeCoords {

    public int x, y, z;

    public int this[int dim] {
        get => dim switch {
            0 => x, 1 => y, _ => z
        };
        set {
            if (dim == 0) x = value;
            else if (dim == 1) y = value;
            else z = value;
        }
    } 

    public static implicit operator CubeCoords((int x, int y, int z) c) => new CubeCoords { x = c.x, y = c.y, z = c.z };
    public static CubeCoords operator +(CubeCoords a, CubeCoords b) => (a.x + b.x, a.y + b.y, a.z + b.z);
    public static CubeCoords operator -(CubeCoords a, CubeCoords b) => (a.x - b.x, a.y - b.y, a.z - b.z);

    public static CubeCoords Min(CubeCoords a, CubeCoords b) => (Math.Min(a.x, b.x), Math.Min(a.y, b.y), Math.Min(a.z, b.z));
    public static CubeCoords Max(CubeCoords a, CubeCoords b) => (Math.Max(a.x, b.x), Math.Max(a.y, b.y), Math.Max(a.z, b.z));

    public static readonly CubeCoords[] Directions = { (0, 0, 1), (0, 0, -1), (0, 1, 0), (0, -1, 0), (1, 0, 0), (-1, 0, 0) };
}
