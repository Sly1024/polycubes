global using Dir3D = System.Int32;
public struct Dir {
    public const int X = 0;
    public const int Y = 1;
    public const int Z = 2;

    public static readonly (int, int)[] Others = new [] { (1, 2), (0, 2), (0, 1) };
}

public record struct Range3D {
    public CubeCoords min;
    public CubeCoords max;

    public Offset1DEnumerator Get1DIndexes(Dir3D dir) {
        return new Offset1DEnumerator(min[dir], max[dir], 1);
    }

    public Offset2DEnumerator Get2DIndexes(Dir3D dir1, Dir3D dir2) {
        return new Offset2DEnumerator(min[dir1], max[dir1], 1, min[dir2], max[dir2], 1);
    }

    public Range3D Project(Dir3D dir, int pos) {
        var copy = this;
        copy.min[dir] = copy.max[dir] = pos;
        return copy;
    }
}

public class Array3D<T> where T : struct {

    public readonly int SizeX;
    public readonly int SizeY;
    public readonly int SizeZ;

    internal T[] array;
    int sizeYZ;

    public readonly int [] sizePerDir;
    public Array3D(int sizeX, int sizeY, int sizeZ)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        SizeZ = sizeZ;

        sizeYZ = sizeY * sizeZ;

        array = new T[sizeX * sizeYZ];
        sizePerDir = new []{ sizeYZ, sizeZ, 1 };
    }

    public T this[int x, int y, int z] {
        get => array[x * sizeYZ + y * SizeZ + z];
        set => array[x * sizeYZ + y * SizeZ + z] = value;
    }

    public T this[CubeCoords c] {
        get => array[c.x * sizeYZ + c.y * SizeZ + c.z];
        set => array[c.x * sizeYZ + c.y * SizeZ + c.z] = value;
    }
    public int GetOffset(Dir3D dir, int pos) => sizePerDir[dir] * pos;

    public Offset1DEnumerator GetOffsets1D(Dir3D dir, int start, int end) {
        var size = sizePerDir[dir];
        return new Offset1DEnumerator(start*size, end*size, end < start ? -size : size);
    }

    public Offset1DEnumerator GetOffsets1D(Dir3D dir, int start, int end, bool backwards) {
        return backwards ? GetOffsets1D(dir, end, start) : GetOffsets1D(dir, start, end);
    }

    public Offset1DEnumerator GetRange1DOffsets(Dir3D dir, Range3D range, bool backwards = false) {
        return GetOffsets1D(dir, range.min[dir], range.max[dir], backwards);
    }

    public Offset2DEnumerator GetOffsets2D(Dir3D dir1, int start1, int end1, Dir3D dir2, int start2, int end2) {
        int size1 = sizePerDir[dir1], size2 = sizePerDir[dir2];
        return new Offset2DEnumerator(start1*size1, end1*size1, end1 < start1 ? -size1 : size1, start2*size2, end2*size2, end2 < start2 ? -size2 : size2);
    }

    public Offset3DEnumerator GetOffsets3D(Dir3D dir1, int start1, int end1, Dir3D dir2, int start2, int end2, Dir3D dir3, int start3, int end3) {
        int size1 = sizePerDir[dir1], size2 = sizePerDir[dir2], size3 = sizePerDir[dir3];
        return new Offset3DEnumerator(
            start1*size1, end1*size1, end1 < start1 ? -size1 : size1,
            start2*size2, end2*size2, end2 < start2 ? -size2 : size2,
            start3*size3, end3*size3, end3 < start3 ? -size3 : size3
        );
    }

    public Offset3DEnumerator GetRange3DOffsets(Range3D range, Dir3D dir1, Dir3D dir2, Dir3D dir3, bool dir1Back = false, bool dir2Back = false, bool dir3Back = false) {
        int start1 = range.min[dir1], end1 = range.max[dir1];
        int start2 = range.min[dir2], end2 = range.max[dir2];
        int start3 = range.min[dir3], end3 = range.max[dir3];
        int size1 = sizePerDir[dir1], size2 = sizePerDir[dir2], size3 = sizePerDir[dir3];
        return new Offset3DEnumerator(
            dir1Back ? end1 : start1, dir1Back ? start1 : end1, dir1Back ? -size1 : size1,
            dir2Back ? end2 : start2, dir2Back ? start2 : end2, dir2Back ? -size2 : size2,
            dir3Back ? end3 : start3, dir3Back ? start3 : end3, dir3Back ? -size3 : size3
        );
    }

    public Offset2DEnumerator GetRange2DOffsets(Dir3D dir1, Dir3D dir2, Range3D range) {
        return GetOffsets2D(dir1, range.min[dir1], range.max[dir1], dir2, range.min[dir2], range.max[dir2]);
    }
}

public record struct Offset1DEnumerator(int start, int end, int inc) {
    public int Current => current;
    private int current;
    private int state = 0;
    
    public bool MoveNext() {
        switch (state) {
            case 0:
                current = start;
                state = 1;
                return true;
            case 1:
                if (current == end) {
                    state = 2;
                    return false;
                }
                current += inc;
                break;
            case 2:
                throw new InvalidOperationException("Enumerator is past the last item");
        }
        
        return true;
    }
    public void Reset() {
        state = 0;
    }

    public Offset1DEnumerator GetEnumerator() {
        return this;
    }
}

public record struct Offset2DEnumerator(int start1, int end1, int inc1, int start2, int end2, int inc2) {
    public (int, int) Current => (current1, current2);
    private int current1, current2;
    private int state = 0;
    
    public bool MoveNext() {
        switch (state) {
            case 0:
                current1 = start1;
                current2 = start2;
                state = 1;
                return true;
            case 1:
                if (current2 == end2) {
                    current2 = start2;
                    if (current1 == end1) {
                        state = 2;
                        return false;
                    }
                    current1 += inc1;
                } else {
                    current2 += inc2;
                }
                return true;
            default:
                throw new InvalidOperationException("Enumerator has finished");
        }
    }
    public void Reset() {
        state = 0;
    }

    public Offset2DEnumerator GetEnumerator() {
        return this;
    }
}

public record struct Offset3DEnumerator(int start1, int end1, int inc1, int start2, int end2, int inc2, int start3, int end3, int inc3) {
    public (int, int, int) Current => (current1, current2, current3);
    private int current1, current2, current3;
    private int state = 0;
    
    public bool MoveNext() {
        switch (state) {
            case 0:
                current1 = start1;
                current2 = start2;
                current3 = start3;
                state = 1;
                return true;
            case 1:
                if (current3 == end3) {
                    current3 = start3;
                    if (current2 == end2) {
                        current2 = start2;
                        if (current1 == end1) {
                            state = 2;
                            return false;
                        }
                        current1 += inc1;
                    } else {
                        current2 += inc2;
                    }
                } else {
                    current3 += inc3;
                }
                return true;
            default:
                state = 0;
                return false;
        }
    }
    public void Reset() {
        state = 0;
    }

    public Offset3DEnumerator GetEnumerator() {
        return this;
    }
}