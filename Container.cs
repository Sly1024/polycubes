public class Container {

    Array3D<bool> cubes3D;
    public readonly List<CubeCoords> cubes = new ();

    public readonly int Size;
    public readonly CubeCoords MidPoint;

    public int CubeCount => cubes.Count;

    ulong[,,] hash3D;
    ulong[,,] stripHash;
    ulong[,] layerHash;

    Range3D bbox;

    public Container(int count)
    {
        Size = count;
        var cnt2 = count * 2;
        cubes3D = new (cnt2-1, cnt2-1, cnt2-1);
        MidPoint = (count-1, count-1, count-1);
        
        bbox = new Range3D { min = (cnt2, cnt2, cnt2), max = (0, 0, 0) };
        
        hash3D = new ulong[Size, Size, Size];
        stripHash = new ulong[3, cnt2-1, cnt2-1];
        layerHash = new ulong[3, cnt2-1];

        GenerateHashes();
    }

    private void GenerateHashes()
    {
        var rnd = new Random(1042);

        for (int x = 0; x < Size; x++) {
            for (int y = 0; y < Size; y++) {
                for (int z = 0; z < Size; z++) {
                    hash3D[x, y, z] = (ulong)rnd.NextInt64();
                }
            }
        }
    }

    public void AddCube(CubeCoords cube)
    {
        cubes3D[cube] = true;
        cubes.Add(cube);

        var updateDir = new TripleTuple<bool>();

        for (int i = 0; i < 3; i++) {
            if (cube[i] < bbox.min[i]) { bbox.min[i] = cube[i]; updateDir[i] = true; }
            if (cube[i] > bbox.max[i]) { bbox.max[i] = cube[i]; updateDir[i] = true; }
        }

        UpdateStripHashes(cube, updateDir);
    }

    public bool this[CubeCoords cube] => cubes3D[cube];
    public bool this[int x, int y, int z] => cubes3D[x, y, z];

    internal void RemoveLastCube() {
        RemoveCubeAt(CubeCount - 1);
    }
    internal void RemoveCubeAt(int idx)
    {
        var cube = cubes[idx];
        cubes3D[cube] = false;
        cubes.RemoveAt(idx);

        var _bbox = bbox;
        var updateDir = new TripleTuple<bool>();

        // adjust BoundingBox
        for (int dir = 0; dir < 3; dir++) {
            if (cube[dir] == _bbox.min[dir] || cube[dir] == _bbox.max[dir]) {
                var foundCube = false;
                
                foreach (var (off1, off2, off3) in cubes3D.GetRange3DOffsets(_bbox.Project(dir, cube[dir]))) {
                    if (cubes3D.array[off1 + off2 + off3]) {
                        foundCube = true;
                        break;
                    }
                } 
                if (!foundCube) {
                    if (cube[dir] == _bbox.min[dir]) { bbox.min[dir]++; updateDir[dir] = true; }
                    if (cube[dir] == _bbox.max[dir]) { bbox.max[dir]--; updateDir[dir] = true; }
                }
            }
        }

        UpdateStripHashes(cube, updateDir);
    }

    private void UpdateStripHashes(CubeCoords cube, TripleTuple<bool> updateDir) {
        for (int dir = 0; dir < 3; dir++) {
            var (dir1, dir2) = Dir.Others[dir];
            if (updateDir[dir]) { // need to update all stripHashes in direction "dir"
                foreach (var (idx1, idx2) in bbox.Get2DIndexes(dir1, dir2)) {
                    int startOff = cubes3D.GetOffset(dir1, idx1) + cubes3D.GetOffset(dir2, idx2);
                    UpdateStripHash(dir, idx1, idx2, startOff);
                }
            } else { // only update the strip that the "cube" is in
                int startOff = cubes3D.GetOffset(dir1, cube[dir1]) + cubes3D.GetOffset(dir2, cube[dir2]);
                UpdateStripHash(dir, cube[dir1], cube[dir2], startOff);
            }
        }
    }

    private bool UpdateStripHash(Dir3D dir, int pos1, int pos2, int startOffset) {
        ulong hash = 0;
        ulong rhash = 0;

        int size = bbox.max[dir] - bbox.min[dir];

        int hi = 0;
        foreach (var offset in cubes3D.GetRange1DOffsets(dir, bbox)) {
            if (cubes3D.array[startOffset + offset]) {
                hash ^= hash3D[0, 0, hi];
                rhash ^= hash3D[0, 0, size - hi];
            }
            hi++;
        }
        stripHash[dir, pos1, pos2] = Math.Min(hash, rhash);

        return false;
    }

    public ulong CalcHash() {
        ulong sum = 0;

        for (int hx = 0, x = bbox.min.x; x <= bbox.max.x; x++, hx++) {
            for (int hy = 0, y = bbox.min.y; y <= bbox.max.y; y++, hy++) {
                for (int hz = 0, z = bbox.min.z; z <= bbox.max.z; z++, hz++) {
                    if (cubes3D[x, y, z]) sum ^= hash3D[hx, hy, hz];
                }
            }
        }

        // for (int x = BBoxMin.x; x <= BBoxMax.x; x++) CalcLayerHash(Dir.X, x);
        // for (int y = BBoxMin.y; y <= BBoxMax.y; y++) CalcLayerHash(Dir.Y, y);
        // for (int z = BBoxMin.z; z <= BBoxMax.z; z++) CalcLayerHash(Dir.Z, z);

        return sum;
    }

    private void CalcLayerHash(Dir3D dir, int p)
    {
        // switch (dir) {
        //     case Dir.X:
        //         var yHashes = new ulong[BBoxMax.y - BBoxMin.y + 1];
        //         var zHashes = new ulong[BBoxMax.z - BBoxMin.z + 1];
        //         for (int i = 0, y = BBoxMin.y; y <= BBoxMax.y; y++, i++) yHashes[i] = stripHash[Dir.Z, p, y];
        //         for (int i = 0, z = BBoxMin.z; z <= BBoxMax.z; z++, i++) zHashes[i] = stripHash[Dir.Y, p, z];
                
        //         var scanDir = DecideScanDirection(yHashes, zHashes);

        //         var temp = new Array3D<bool>(4, 4, 4);

        //         var (pStart, pEnd, pInc) = temp.LoopVars(Dir.Y, BBoxMin.y, BBoxMax.y, scanDir.PrimaryBackwards);
        //         var (sStart, sEnd, sInc) = temp.LoopVars(Dir.Z, BBoxMin.z, BBoxMax.z, scanDir.SecondaryBackwards);
        //         if (scanDir.SecondaryFirst) {
        //             (pStart, pEnd, pInc, sStart, sEnd, sInc) = (sStart, sEnd, sInc, pStart, pEnd, pInc);
        //         }

        //         ulong hash = 0;

        //         var offset = temp.OffsetX(p);
        //         for (int hx = 0, pi = pStart; pi != pEnd; pi += pInc, hx++) {
        //             for (int hy = 0, si = sStart; si != sEnd; si += sInc, hy++) {
        //                 if (temp.array[pi+si+offset]) {
        //                     hash ^= hash3D[hx, hy, 0];
        //                 }
        //             }
        //         }

        //         foreach (var off in temp.GetOffsets1D(0, 0, 0)) {

        //         }

        //         break;
        //     case Dir.Y:
        //         break;
        //     case Dir.Z:
        //         break;
        // }


    }

    const int ScanDir_SecondaryFirst = 1;
    const int ScanDir_PrimaryBackwards = 2;
    const int ScanDir_SecondaryBackwards = 4;

    private (bool SecondaryFirst, bool PrimaryBackwards, bool SecondaryBackwards) DecideScanDirection(ulong[] pHashes, ulong[] sHashes)
    {
        var pLen = pHashes.Length;
        bool pBack = false, sBack = false;

        for (int iStart = 0, iEnd = pLen-1; iStart < iEnd ; iStart++, iEnd--) {
            if (pHashes[iStart] != pHashes[iEnd]) {
                if (pHashes[iStart] > pHashes[iEnd]) pBack = true;
                break;
            }
        }

        for (int iStart = 0, iEnd = sHashes.Length-1; iStart < iEnd ; iStart++, iEnd--) {
            if (sHashes[iStart] != sHashes[iEnd]) {
                if (sHashes[iStart] > sHashes[iEnd]) sBack = true;
                break;
            }
        }

        if (pLen < sHashes.Length) {
            return (false, pBack, sBack);
        }
        if (pLen > sHashes.Length) {
            return (true, pBack, sBack);
        }

        for (int i = 0; i < pLen; i++) {
            var diff = pHashes[pBack ? pLen-1-i : i] - sHashes[sBack ? pLen-1-i : i];
            if (diff > 0) return (true, pBack, sBack);
            if (diff < 0) return (false, pBack, sBack);
        }

        return (false, pBack, sBack);
    }
}