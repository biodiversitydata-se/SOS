using FluentAssertions;
using NetTopologySuite.Geometries;
using SOS.Lib.Extensions;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class GISExtensionsTests
{
    private static Polygon ValidPolygon => new(new LinearRing(
    [
        new Coordinate(50, 50),
        new Coordinate(60, 50),
        new Coordinate(60, 60),
        new Coordinate(50, 60),
        new Coordinate(50, 50)
    ]));

    /// <summary>
    /// Tests that TryMakeValid does not throw for any problematic polygon wrapped in a MultiPolygon.
    /// This is the exact scenario from the reported bug.
    /// </summary>
    [Theory]
    [MemberData(nameof(ProblematicMultiPolygonCases))]
    public void TryMakeValid_MultiPolygonWithProblematicComponent_ShouldNotThrow(
        string caseName, MultiPolygon multiPolygon)
    {
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var act = () => multiPolygon.TryMakeValid();

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------
        act.Should().NotThrow(
            $"case '{caseName}' should be handled gracefully without ArgumentException");
    }
   
    public static TheoryData<string, MultiPolygon> ProblematicMultiPolygonCases()
    {
        var data = new TheoryData<string, MultiPolygon>();
        foreach (var (name, polygon) in GetProblematicPolygons())
        {
            data.Add(name, new MultiPolygon([polygon, ValidPolygon]));
        }
        return data;
    }

    public static TheoryData<string, Polygon> ProblematicSinglePolygonCases()
    {
        var data = new TheoryData<string, Polygon>();
        foreach (var (name, polygon) in GetProblematicPolygons())
        {
            data.Add(name, polygon);
        }
        return data;
    }

    private static IEnumerable<(string Name, Polygon Polygon)> GetProblematicPolygons()
    {
        // ----- Self-intersecting (bowtie / figure-eight variants) -----

        // 1. Classic bowtie — edges cross at (1,1)
        yield return ("Bowtie_Small", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(2, 0),
            new Coordinate(0, 2), new Coordinate(2, 2),
            new Coordinate(0, 0)
        ])));

        // 2. Larger bowtie — same topology, larger area
        yield return ("Bowtie_Large", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(100, 0),
            new Coordinate(0, 100), new Coordinate(100, 100),
            new Coordinate(0, 0)
        ])));

        // 3. Figure-eight — horizontal crossing at (5,5)
        yield return ("FigureEight", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(10, 10),
            new Coordinate(10, 0), new Coordinate(0, 10),
            new Coordinate(0, 0)
        ])));

        // 4. Figure-eight with realistic Swedish WGS84 coordinates
        yield return ("FigureEight_Swedish", new Polygon(new LinearRing(
        [
            new Coordinate(18.0, 59.0), new Coordinate(18.1, 59.1),
            new Coordinate(18.1, 59.0), new Coordinate(18.0, 59.1),
            new Coordinate(18.0, 59.0)
        ])));

        // 5. Pentagram (star) — five self-intersections
        yield return ("Pentagram", new Polygon(new LinearRing(
        [
            new Coordinate(5, 0),   new Coordinate(2, 9.5),
            new Coordinate(9.5, 3), new Coordinate(0.5, 3),
            new Coordinate(8, 9.5), new Coordinate(5, 0)
        ])));

        // 6. Double crossing (complex self-intersection)
        yield return ("DoubleCrossing", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(3, 6),
            new Coordinate(6, 0), new Coordinate(6, 6),
            new Coordinate(0, 6), new Coordinate(3, 0),
            new Coordinate(0, 0)
        ])));

        // ----- Degenerate (zero-area or collapsing) -----

        // 7. Collinear points — zero area (line masquerading as polygon)
        yield return ("Collinear", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(5, 0),
            new Coordinate(10, 0), new Coordinate(0, 0)
        ])));

        // 8. Near-collinear — extremely thin polygon
        yield return ("NearlyCollinear", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(10, 0.0000001),
            new Coordinate(0, 0.0000001), new Coordinate(10, 0),
            new Coordinate(0, 0)
        ])));

        // 9. Spike — zero-width protrusion (goes out and comes back)
        yield return ("Spike", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(10, 0),
            new Coordinate(10, 10), new Coordinate(5, 10),
            new Coordinate(5, 20), new Coordinate(5.0001, 10),
            new Coordinate(0, 10), new Coordinate(0, 0)
        ])));

        // ----- Holes that break topology -----

        // 10. Hole extends beyond the shell (invalid topology)
        yield return ("HoleExtendsOutside", new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0), new Coordinate(10, 0),
                new Coordinate(10, 10), new Coordinate(0, 10),
                new Coordinate(0, 0)
            ]),
            [
                new LinearRing(
                [
                    new Coordinate(3, -2), new Coordinate(7, -2),
                    new Coordinate(7, 12), new Coordinate(3, 12),
                    new Coordinate(3, -2)
                ])
            ]));

        // 11. Hole cuts shell into two disconnected parts
        yield return ("HoleSplitsShell", new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0), new Coordinate(20, 0),
                new Coordinate(20, 10), new Coordinate(0, 10),
                new Coordinate(0, 0)
            ]),
            [
                new LinearRing(
                [
                    new Coordinate(9, -1), new Coordinate(11, -1),
                    new Coordinate(11, 11), new Coordinate(9, 11),
                    new Coordinate(9, -1)
                ])
            ]));

        // 12. Two overlapping holes
        yield return ("OverlappingHoles", new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0), new Coordinate(20, 0),
                new Coordinate(20, 20), new Coordinate(0, 20),
                new Coordinate(0, 0)
            ]),
            [
                new LinearRing(
                [
                    new Coordinate(3, 3), new Coordinate(12, 3),
                    new Coordinate(12, 12), new Coordinate(3, 12),
                    new Coordinate(3, 3)
                ]),
                new LinearRing(
                [
                    new Coordinate(8, 8), new Coordinate(17, 8),
                    new Coordinate(17, 17), new Coordinate(8, 17),
                    new Coordinate(8, 8)
                ])
            ]));

        // ----- Mixed: self-intersecting shell + holes -----

        // 13. Bowtie shell with a valid hole
        yield return ("BowtieWithHole", new Polygon(
            new LinearRing(
            [
                new Coordinate(0, 0), new Coordinate(10, 10),
                new Coordinate(10, 0), new Coordinate(0, 10),
                new Coordinate(0, 0)
            ]),
            [
                new LinearRing(
                [
                    new Coordinate(2, 2), new Coordinate(4, 2),
                    new Coordinate(4, 4), new Coordinate(2, 4),
                    new Coordinate(2, 2)
                ])
            ]));

        // ----- Edge cases from real-world user JSON input -----

        // 14. Polygon with repeated sequential coordinates (copy-paste errors in JSON)
        yield return ("RepeatedVertices", new Polygon(new LinearRing(
        [
            new Coordinate(0, 0), new Coordinate(10, 0), new Coordinate(10, 0),
            new Coordinate(10, 10), new Coordinate(10, 10), new Coordinate(10, 10),
            new Coordinate(0, 10), new Coordinate(0, 0)
        ])));

        // 15. Polygon where most coordinates collapse to 2 unique (after HashSet dedup)
        yield return ("CollapsesToTwoPoints", new Polygon(new LinearRing(
        [
            new Coordinate(1, 1), new Coordinate(2, 2),
            new Coordinate(1, 1), new Coordinate(2, 2),
            new Coordinate(1, 1)
        ])));
    }
}