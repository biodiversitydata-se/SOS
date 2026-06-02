using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Gis;

public enum LocationClusteringAlgorithm
{
    Sweref99TmFastGreedy = 1,
    Sweref99TmGreedy = 2,
    DbScan = 3
}