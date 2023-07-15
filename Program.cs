public class PolyCubes {
    
    public static void Main() {
        var generator = new Generator(3);
        generator.Generate();
        System.Console.WriteLine(generator.counter);

        // var t = new Array3D<bool>( 4, 5, 3);

        // foreach (var offx in t.GetOffsets(Dir.X, 0, 3))
        // {
        //     foreach (var offy in t.GetOffsets(Dir.Y, 4, 0))
        //     {
        //         foreach (var offz in t.GetOffsets(Dir.Z, 0, 2))
        //         {
        //             System.Console.WriteLine(offx + offy + offz);
        //             // System.Console.WriteLine(offy);
        //         }
        //     }
        // }
    }
}
