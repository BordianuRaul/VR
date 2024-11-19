using System;


namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }
        
        
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            Vector O = line.X0 - Center;  
            Vector D = line.Dx;  
            
            double a = SemiAxesLength.X;
            double b = SemiAxesLength.Y;
            double c = SemiAxesLength.Z;
            
            double A = (D.X * D.X) / (a * a) + (D.Y * D.Y) / (b * b) + (D.Z * D.Z) / (c * c);
            double B = 2 * ((O.X * D.X) / (a * a) + (O.Y * D.Y) / (b * b) + (O.Z * D.Z) / (c * c));
            double C = (O.X * O.X) / (a * a) + (O.Y * O.Y) / (b * b) + (O.Z * O.Z) / (c * c) - Radius * Radius;
            
            double discriminant = B * B - 4 * A * C;

            if (discriminant < 0)
            {
                return Intersection.NONE;
            }

            bool visible = true;
            
            double sqrtDiscriminant = Math.Sqrt(discriminant);
            double t1 = (-B - sqrtDiscriminant) / (2 * A);
            double t2 = (-B + sqrtDiscriminant) / (2 * A);
            
            if (t1 >= minDist && t1 <= maxDist)
            {
                Vector intersectionPoint = line.CoordinateToPosition(t1);
                
                Vector normal = new Vector(
                    (intersectionPoint.X - Center.X) / (a * a),
                    (intersectionPoint.Y - Center.Y) / (b * b),
                    (intersectionPoint.Z - Center.Z) / (c * c)
                );
                normal.Normalize();

                return new Intersection(
                    true,                
                    true,                 
                    this,               
                    line,                
                    t1,               
                    normal,               
                    Material,         
                    Color          
                );
            }

            if (t2 >= minDist && t2 <= maxDist)
            {
                Vector intersectionPoint = line.CoordinateToPosition(t2);
                
                Vector normal = new Vector(
                    (intersectionPoint.X - Center.X) / (a * a),
                    (intersectionPoint.Y - Center.Y) / (b * b),
                    (intersectionPoint.Z - Center.Z) / (c * c)
                );
                normal.Normalize();

                return new Intersection(
                    true,              
                    true,           
                    this,                 
                    line,                  
                    t2,                   
                    normal,                
                    Material,        
                    Color            
                );
            }
            
            return Intersection.NONE;
        }
        
        public Vector GetCenter() { return Center; }
    }
}
