﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests for reading WKB.
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixture]
    public class WKBReaderTest
    {
        [Test]
        public void TestPolygonEmpty()
        {
            var reader = new WKTReader();
            var geom = reader.Read("POLYGON EMPTY");
            CheckWkbGeometry(geom.AsBinary(), "POLYGON EMPTY");
        }

        [Test]
        public void TestShortPolygons()
        {
            // one point
            CheckWkbGeometry("0000000003000000010000000140590000000000004069000000000000",
                             "POLYGON ((100 200, 100 200, 100 200, 100 200))");
            // two point
            CheckWkbGeometry(
                "000000000300000001000000024059000000000000406900000000000040590000000000004069000000000000",
                "POLYGON ((100 200, 100 200, 100 200, 100 200))");
        }

        [Test]
        public void TestSinglePointLineString()
        {
            CheckWkbGeometry("00000000020000000140590000000000004069000000000000",
                             "LINESTRING (100 200, 100 200)");
        }

        /// <summary>
        /// After removing the 39 bytes of MBR info at the front, and the
        /// end-of-geometry byte, * Spatialite native BLOB is very similar
        /// to WKB, except instead of a endian marker at the start of each
        /// geometry in a multi-geometry, it has a start marker of 0x69.
        /// Endianness is determined by the endian value of the multigeometry.
        /// </summary>
        [Test]
        public void TestSpatialiteMultiGeometry()
        {
            //multipolygon
            CheckWkbGeometry(
                "01060000000200000069030000000100000004000000000000000000444000000000000044400000000000003440000000000080464000000000008046400000000000003E4000000000000044400000000000004440690300000001000000040000000000000000003E40000000000000344000000000000034400000000000002E40000000000000344000000000000039400000000000003E400000000000003440",
                "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((30 20, 20 15, 20 25, 30 20)))");

            //multipoint
            CheckWkbGeometry(
                "0104000000020000006901000000000000000000F03F000000000000F03F690100000000000000000000400000000000000040",
                "MULTIPOINT(1 1, 2 2)");

            //multiline
            CheckWkbGeometry(
                "010500000002000000690200000003000000000000000000244000000000000024400000000000003440000000000000344000000000000024400000000000004440690200000004000000000000000000444000000000000044400000000000003E400000000000003E40000000000000444000000000000034400000000000003E400000000000002440",
                "MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");

            //geometrycollection
            CheckWkbGeometry(
                "010700000002000000690100000000000000000010400000000000001840690200000002000000000000000000104000000000000018400000000000001C400000000000002440",
                "GEOMETRYCOLLECTION(POINT(4 6),LINESTRING(4 6,7 10))");
        }

        [Test]
        public void Test2dSpatialiteWKB()
        {
            // Point
            CheckWkbGeometry("0101000020E6100000000000000000F03F0000000000000040",
        "POINT(1 2)");
            // LineString
            CheckWkbGeometry("0102000020E610000002000000000000000000F03F000000000000004000000000000008400000000000001040",
            "LINESTRING(1 2, 3 4)");
            // Polygon
            CheckWkbGeometry("0103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F",
                "POLYGON((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1))");
            // MultiPoint
            CheckWkbGeometry("0104000020E61000000200000001010000000000000000000000000000000000F03F010100000000000000000000400000000000000840",
                "MULTIPOINT(0 1,2 3)");
            // MultiLineString
            CheckWkbGeometry("0105000020E6100000020000000102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C40",
                "MULTILINESTRING((0 1,2 3),(4 5,6 7))");

            string multiPolygonWkt = "MULTIPOLYGON(((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),((-9 0,-9 10,-1 10,-1 0,-9 0)))";
            // MultiPolygon with non-compact WKB
            CheckWkbGeometry("0106000020E6100000020000000103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000020E6100000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", multiPolygonWkt);
            // MultiPolygon with compact WKB
            CheckWkbGeometry("0106000020E61000000200000001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", multiPolygonWkt);

            string geometryCollectionWkt = "GEOMETRYCOLLECTION(POINT(0 1),POINT(0 1),POINT(2 3),LINESTRING(2 3,4 5),LINESTRING(0 1,2 3),LINESTRING(4 5,6 7),POLYGON((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),POLYGON((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),POLYGON((-9 0,-9 10,-1 10,-1 0,-9 0)))";
            // GeometryCollection with non-compact WKB
            CheckWkbGeometry("0107000020E6100000090000000101000020E61000000000000000000000000000000000F03F0101000020E61000000000000000000000000000000000F03F0101000020E6100000000000000000004000000000000008400102000020E61000000200000000000000000000400000000000000840000000000000104000000000000014400102000020E6100000020000000000000000000000000000000000F03F000000000000004000000000000008400102000020E6100000020000000000000000001040000000000000144000000000000018400000000000001C400103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000020E6100000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", geometryCollectionWkt);
            // GeometryCollection with compact WKB
            CheckWkbGeometry("0107000020E61000000900000001010000000000000000000000000000000000F03F01010000000000000000000000000000000000F03F01010000000000000000000040000000000000084001020000000200000000000000000000400000000000000840000000000000104000000000000014400102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C4001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F01030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", geometryCollectionWkt);
        }

        [Test]
        public void TestSpatialiteWKB_Z()
        {
            // PointZ
            CheckWkbGeometry("01010000A0E6100000000000000000F03F00000000000000400000000000000840",
            "POINT Z(1 2 3)");
            // LineStringZ
            CheckWkbGeometry("01020000A0E610000002000000000000000000F03F00000000000000400000000000000840000000000000104000000000000014400000000000001840",
            "LINESTRING Z(1 2 3, 4 5 6)");
            // PolygonZ
            CheckWkbGeometry("01030000A0E6100000020000000500000000000000000000000000000000000000000000000000594000000000000000000000000000002440000000000000594000000000000024400000000000002440000000000000594000000000000024400000000000000000000000000000594000000000000000000000000000000000000000000000594005000000000000000000F03F000000000000F03F0000000000005940000000000000F03F000000000000224000000000000059400000000000002240000000000000224000000000000059400000000000002240000000000000F03F0000000000005940000000000000F03F000000000000F03F0000000000005940",
            "POLYGON Z((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100))");
            // MultiPointZ
            CheckWkbGeometry("01040000A0E61000000200000001010000800000000000000000000000000000F03F00000000000000400101000080000000000000084000000000000010400000000000001440",
            "MULTIPOINTS Z(0 1 2, 3 4 5)");
            // MultiLineStringZ
            CheckWkbGeometry("01050000A0E6100000020000000102000080020000000000000000000000000000000000F03F000000000000004000000000000008400000000000001040000000000000144001020000800200000000000000000018400000000000001C400000000000002040000000000000224000000000000024400000000000002640",
            "MULTILINESTRING Z((0 1 2,3 4 5),(6 7 8,9 10 11))");
            // MultiPolygonZ
            CheckWkbGeometry("01060000A0E6100000020000000103000080020000000500000000000000000000000000000000000000000000000000594000000000000000000000000000002440000000000000594000000000000024400000000000002440000000000000594000000000000024400000000000000000000000000000594000000000000000000000000000000000000000000000594005000000000000000000F03F000000000000F03F0000000000005940000000000000F03F000000000000224000000000000059400000000000002240000000000000224000000000000059400000000000002240000000000000F03F0000000000005940000000000000F03F000000000000F03F00000000000059400103000080010000000500000000000000000022C00000000000000000000000000000494000000000000022C000000000000024400000000000004940000000000000F0BF00000000000024400000000000004940000000000000F0BF0000000000000000000000000000494000000000000022C000000000000000000000000000004940",
            "MULTIPOLYGON Z(((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100)),((-9 0 50,-9 10 50,-1 10 50,-1 0 50,-9 0 50)))");
            // GeometryCollectionZ
        }


        [Test]
        public void TestSpatialiteWKB_M()
        {
            // PointM
            CheckWkbGeometry("0101000060E6100000000000000000F03F00000000000000400000000000000840",
            "POINT M(1 2 3)");
            // LineStringM
            CheckWkbGeometry("0102000060E610000002000000000000000000F03F00000000000000400000000000000840000000000000104000000000000014400000000000001840",
            "LINESTRING M(1 2 3,4 5 6)");
            // PolygonM
            CheckWkbGeometry("0103000060E6100000020000000500000000000000000000000000000000000000000000000000594000000000000000000000000000002440000000000000594000000000000024400000000000002440000000000000594000000000000024400000000000000000000000000000594000000000000000000000000000000000000000000000594005000000000000000000F03F000000000000F03F0000000000005940000000000000F03F000000000000224000000000000059400000000000002240000000000000224000000000000059400000000000002240000000000000F03F0000000000005940000000000000F03F000000000000F03F0000000000005940",
            "POLYGON M((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100))");
            // MultiPointM
            CheckWkbGeometry("01040000A0E61000000200000001010000800000000000000000000000000000F03F00000000000000400101000080000000000000084000000000000010400000000000001440",
            "MULTIPOINT M(0 1 2,3 4 5)");
            // MultiLineStringM
            CheckWkbGeometry("0105000060E6100000020000000102000040020000000000000000000000000000000000F03F000000000000004000000000000008400000000000001040000000000000144001020000400200000000000000000018400000000000001C400000000000002040000000000000224000000000000024400000000000002640",
            "MULTILINESTRING M((0 1 2,3 4 5),(6 7 8,9 10 11))");
            // MultiPolygonM
            CheckWkbGeometry("0106000060E6100000020000000103000040020000000500000000000000000000000000000000000000000000000000594000000000000000000000000000002440000000000000594000000000000024400000000000002440000000000000594000000000000024400000000000000000000000000000594000000000000000000000000000000000000000000000594005000000000000000000F03F000000000000F03F0000000000005940000000000000F03F000000000000224000000000000059400000000000002240000000000000224000000000000059400000000000002240000000000000F03F0000000000005940000000000000F03F000000000000F03F00000000000059400103000040010000000500000000000000000022C00000000000000000000000000000494000000000000022C000000000000024400000000000004940000000000000F0BF00000000000024400000000000004940000000000000F0BF0000000000000000000000000000494000000000000022C000000000000000000000000000004940",
            "MULTIPOLYGON M(((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100)),((-9 0 50,-9 10 50,-1 10 50,-1 0 50,-9 0 50)))");
            // GeometryCollectionM
            //CheckWkbGeometry("0107000020E61000000900000001010000000000000000000000000000000000F03F01010000000000000000000000000000000000F03F01010000000000000000000040000000000000084001020000000200000000000000000000400000000000000840000000000000104000000000000014400102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C4001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F01030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000",
            //    "MULTIPOLYGONM(((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100)),((-9 0 50,-9 10 50,-1 10 50,-1 0 50,-9 0 50)))");
        }

        [Test]
        public void TestSpatialiteWKB_ZM()
        {
            // PointZM
            CheckWkbGeometry("01010000E0E6100000000000000000F03F000000000000004000000000000008400000000000006940",
            "POINT ZM (1 2 3 200)");
            // LineStringZM
            CheckWkbGeometry("01020000E0E610000002000000000000000000F03F0000000000000040000000000000084000000000000069400000000000001040000000000000144000000000000018400000000000006940",
            "LINESTRING ZM (1 2 3 200,4 5 6 200)");
            // PolygonZM
            CheckWkbGeometry("01030000E0E610000002000000050000000000000000000000000000000000000000000000000059400000000000006940000000000000000000000000000024400000000000005940000000000000694000000000000024400000000000002440000000000000594000000000000069400000000000002440000000000000000000000000000059400000000000006940000000000000000000000000000000000000000000005940000000000000694005000000000000000000F03F000000000000F03F00000000000059400000000000006940000000000000F03F00000000000022400000000000005940000000000000694000000000000022400000000000002240000000000000594000000000000069400000000000002240000000000000F03F00000000000059400000000000006940000000000000F03F000000000000F03F00000000000059400000000000006940",
            "POLYGON ZM ((0 0 100 200,0 10 100 200,10 10 100 200,10 0 100 200,0 0 100 200),(1 1 100 200,1 9 100 200,9 9 100 200,9 1 100 200,1 1 100 200))");
            // MultiPointZM
            CheckWkbGeometry("01040000E0E61000000200000001010000C00000000000000000000000000000F03F0000000000000040000000000000694001010000C00000000000000840000000000000104000000000000014400000000000006940",
            "MULTIPOINT ZM (0 1 2 200,3 4 5 200)");
            // MultiLineStringZM
            CheckWkbGeometry("01050000E0E61000000200000001020000C0020000000000000000000000000000000000F03F00000000000000400000000000006940000000000000084000000000000010400000000000001440000000000000694001020000C00200000000000000000018400000000000001C40000000000000204000000000000069400000000000002240000000000000244000000000000026400000000000006940",
            "MULTILINESTRING ZM ((0 1 2 200,3 4 5 200),(6 7 8 200,9 10 11 200))");
            // MultiPolygonZM
            CheckWkbGeometry("01060000E0E61000000200000001030000C002000000050000000000000000000000000000000000000000000000000059400000000000006940000000000000000000000000000024400000000000005940000000000000694000000000000024400000000000002440000000000000594000000000000069400000000000002440000000000000000000000000000059400000000000006940000000000000000000000000000000000000000000005940000000000000694005000000000000000000F03F000000000000F03F00000000000059400000000000006940000000000000F03F00000000000022400000000000005940000000000000694000000000000022400000000000002240000000000000594000000000000069400000000000002240000000000000F03F00000000000059400000000000006940000000000000F03F000000000000F03F0000000000005940000000000000694001030000C0010000000500000000000000000022C000000000000000000000000000004940000000000000694000000000000022C0000000000000244000000000000049400000000000006940000000000000F0BF000000000000244000000000000049400000000000006940000000000000F0BF00000000000000000000000000004940000000000000694000000000000022C0000000000000000000000000000049400000000000006940",
            "MULTIPOLYGON ZM (((0 0 100 200,0 10 100 200,10 10 100 200,10 0 100 200,0 0 100 200),(1 1 100 200,1 9 100 200,9 9 100 200,9 1 100 200,1 1 100 200)),((-9 0 50 200,-9 10 50 200,-1 10 50 200,-1 0 50 200,-9 0 50 200)))");
            // GeometryCollectionZM
            //CheckWkbGeometry("0107000020E61000000900000001010000000000000000000000000000000000F03F01010000000000000000000000000000000000F03F01010000000000000000000040000000000000084001020000000200000000000000000000400000000000000840000000000000104000000000000014400102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C4001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F01030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000",
            //    "MULTIPOLYGONM(((0 0 100,0 10 100,10 10 100,10 0 100,0 0 100),(1 1 100,1 9 100,9 9 100,9 1 100,1 1 100)),((-9 0 50,-9 10 50,-1 10 50,-1 0 50,-9 0 50)))");
        }

        [Test]
        public void TestSRIDInSubGeometry()
        {
            // MultiPolygon
            CheckSRID("0106000020E61000000200000001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", 4326);
            // GeometryCollection
            CheckSRID("0107000020E61000000900000001010000000000000000000000000000000000F03F01010000000000000000000000000000000000F03F01010000000000000000000040000000000000084001020000000200000000000000000000400000000000000840000000000000104000000000000014400102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C4001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F01030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000", 4326);
        }

        [Test]
        public void TestInvalidWkbShouldBeReadable()
        {
            var wkbReader = new WKBReader();
            // The last sub-geometry uses 2029 unlike others.
            var geometry = wkbReader.Read(WKBReader.HexToBytes("0107000020E6100000090000000101000020E61000000000000000000000000000000000F03F0101000020E61000000000000000000000000000000000F03F0101000020E6100000000000000000004000000000000008400102000020E61000000200000000000000000000400000000000000840000000000000104000000000000014400102000020E6100000020000000000000000000000000000000000F03F000000000000004000000000000008400102000020E6100000020000000000000000001040000000000000144000000000000018400000000000001C400103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000020E61000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000020ED070000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000"));

            Assert.That(geometry, Is.InstanceOf<GeometryCollection>());
            Assert.That(geometry.SRID, Is.EqualTo(4326));

            var geometryCollection = (GeometryCollection)geometry;
            for (int i = 0; i < geometryCollection.NumGeometries - 1; i++)
            {
                Assert.That(geometryCollection.GetGeometryN(i).SRID, Is.EqualTo(4326));
            }
            var lastSubGeometry = geometryCollection.GetGeometryN(geometryCollection.NumGeometries - 1);
            Assert.That(lastSubGeometry, Is.InstanceOf<Polygon>());
            Assert.That(lastSubGeometry.SRID, Is.EqualTo(2029));
        }

        /// <summary>
        /// Tests WKB that requests a huge number of points.
        /// </summary>
        [Test]
        public void TestHugeNumberOfPoints()
        {
            /*
             * 0: 00 - XDR (Big endian)
             * 1: 00000003 - POLYGON ( 3 ) 
             * 5: 00000001 - Num Rings = 1
             * 9: 40590000 - Num Points = 1079574528
             */
            CheckWkbParseException("00000000030000000140590000000000004069000000000000");
        }

        [Test]
        public void TestNumCoordsNegative()
        {
            /*
             * 0: 01 - NDR (Little endian)
             * 1: 02000000 - LINESTRING ( 2 ) 
             * 5: 0000FFFF - Num Points = -65536     * 0: 00 - XDR (Big endian)
             */
            CheckWkbParseException("01020000000000FFFF");
        }

        [Test]
        public void TestNumElementsNegative()
        {
            /*
             * 0: 00 - XDR (Big endian)
             * 1: 00000004 - MULTIPOINT ( 4 ) 
             * 5: FFFFFFFF - Num Elements = -1     * 0: 01 - NDR (Little endian)
             */
            CheckWkbParseException("0000000004FFFFFFFF000000000140590000000000004059000000000000000000000140690000000000004059000000000000");
        }

        [Test]
        public void TestNumRingsNegative()
        {
            /*
             * 0: 00 - XDR (Big endian)
             * 1: 00000004 - MULTIPOINT ( 4 ) 
             * 5: FFFFFFFF - Num Elements = -1     * 0: 01 - NDR (Little endian)
             */
            CheckWkbParseException("0000000003FFFFFFFF0000000440590000000000004069000000000000405900000000000040590000000000004069000000000000405900000000000040590000000000004069000000000000");
        }

        //======================================

        private void CheckWkbParseException(string wkbHex)
        {
            try
            {
                CheckWkbGeometry(wkbHex, "");
            }
            catch (ParseException e)
            {
                // all good
                return;
            }
            // expected ParseException did not occur
            Assert.Fail("Expected ParseException");
        }

        private static void CheckWkbGeometry(string wkbHex, string expectedWKT)
        {
            CheckWkbGeometry(WKBReader.HexToBytes(wkbHex), expectedWKT);
        }

        private static void CheckWkbGeometry(byte[] wkb, string expectedWKT)
        {
            var wkbReader = new WKBReader();
            var g2 = wkbReader.Read(wkb);

            // JTS deviation: our default reader doesn't do Z by default, so in addition to XYM and
            // XYZM, we also need to use a special reader for XYZ.
            var useReader = new WKTReader();
            //if (Regex.IsMatch(expectedWKT, "(Z|(Z?M)) ?\\("))
            //{
            //    useReader = new WKTReader(NtsGeometryServices.Instance.CreateGeometryFactory(PackedCoordinateSequenceFactory.DoubleFactory));
            //}

            var expected = useReader.Read(expectedWKT);

            bool isEqual = (expected.CompareTo(g2 /*, Comp2*/) == 0);
            Assert.IsTrue(isEqual);

        }

        private void CheckSRID(string wkbHex, int expectedSrid)
        {
            var wkbReader = new WKBReader();
            var geometry = wkbReader.Read(WKBReader.HexToBytes(wkbHex));

            Assert.That(geometry, Is.InstanceOf<GeometryCollection>());
            Assert.That(geometry.SRID, Is.EqualTo(expectedSrid));

            var geometryCollection = (GeometryCollection)geometry;
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
                Assert.That(geometryCollection.GetGeometryN(i).SRID, Is.EqualTo(expectedSrid));
        }

        [Test]
        public void TestBase64TextFiles()
        {
            // taken from: https://raw.githubusercontent.com/SharpMap/SharpMap/5289522c26e77584eaa95428c1bd2202ff18a340/UnitTests/TestData/Base%2064.txt
            TestBase64TextFile(EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.Base 64.txt"));
        }

        private static void TestBase64TextFile(Stream file)
        {
            byte[] wkb = ConvertBase64(file);
            var wkbReader = new WKBReader();
            Geometry geom = null;
            Assert.DoesNotThrow(() => geom = wkbReader.Read(wkb));
        }

        private static byte[] ConvertBase64(Stream file)
        {
            using (var sr = new StreamReader(file))
            {
                return Convert.FromBase64String(sr.ReadToEnd());
            }
        }
    }
}
