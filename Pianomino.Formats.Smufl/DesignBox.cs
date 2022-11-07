using System;
using System.Collections.Generic;
using System.Text;

namespace Pianomino.Formats.Smufl;

public readonly struct DesignBox
{
    public readonly DesignPoint SouthWest;
    public readonly DesignPoint NorthEast;

    public DesignBox(DesignPoint southWest, DesignPoint northEast)
        => (SouthWest, NorthEast) = (southWest, northEast);
}
