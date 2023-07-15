public class Generator {

    Container container;

    public int counter;
    private HashSet<ulong> seen;

    public Generator(int count)
    {
        container = new Container(count);
        counter = 0;
        seen = new ();
    }

    public void Generate() {
        container.AddCube(container.MidPoint);
        TryAddNextCube();
    }

    private void TryAddNextCube()
    {
        if (container.CubeCount == container.Size) {
            var hash = container.CalcHash();
            if (!seen.Contains(hash)) {
                counter++;
                seen.Add(hash);
            }
            return;
        }

        foreach (var cube in container.cubes.ToArray()) {
            foreach (var dir in CubeCoords.Directions) {
                var location = cube + dir;
                if (!container[location]) {
                    container.AddCube(location);
                    TryAddNextCube();
                    container.RemoveLastCube();
                }
            }
        }
    }

}