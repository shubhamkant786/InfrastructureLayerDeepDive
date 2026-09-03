using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InfrastructureLayerDeepDive.Weather.Domain
{
    public class LocationEntity
    {
        public Guid LocationId { get; set; }
        //A Point is a structure that represents an ordered pair of integer x- and y-coordinates that defines a point in a two-dimensional plane.
        //Point is taken from NetTopologySuite.Geometries namespace. It is used to represent a point in a two-dimensional space.
        public Point Point { get; set; }
    }
}
