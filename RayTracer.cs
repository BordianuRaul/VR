using System;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            Line ray = new Line(point, light.Position);

            var maxDist = (light.Position - point).Length() + 1.0d;
            
            Intersection intersect = FindFirstIntersection(ray, 0.0001d, maxDist);
            
            if (intersect.Geometry is RawCtMask)
            {
                return true; 
            }
            return !(intersect.Valid);
        }


        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color(0.2, 0.2, 0.2, 1.0);

            var viewParallel = (camera.Up ^ camera.Direction).Normalize();

            var image = new Image(width, height);

            var cameraPosition = camera.Position;
            var vecW = camera.Direction * camera.ViewPlaneDistance;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {

                    Vector rayDirection = cameraPosition + vecW + viewParallel * ImageToViewPlane(i, width, camera.ViewPlaneWidth) + camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight);
                    Line ray = new Line(cameraPosition, rayDirection);
                    Intersection intersect = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    if (intersect.Valid && intersect.Visible)
                    {
                        Color color = new Color();
                        foreach (var light in lights)
                        {
                            color += intersect.Material.Ambient * light.Ambient;
                            if (IsLit(intersect.Position, light))
                            {
                                Vector N = intersect.Normal;
                                Vector T = (light.Position - intersect.Position).Normalize();
                                Vector E = (camera.Position - intersect.Position).Normalize();
                                Vector R = (N * (N * T) * 2 - T).Normalize();
                                if (N * T > 0.0d)
                                {
                                    color += intersect.Material.Diffuse * light.Diffuse * (N * T);
                                }
                                if (E * R > 0.0d)
                                {
                                    color += intersect.Material.Specular * light.Specular * Math.Pow(E * R, intersect.Material.Shininess);
                                }
                                
                            }
                            
                        }
                        
                        image.SetPixel(i, j, color);
                    }
                    else
                    {
                        image.SetPixel(i, j, background);
                    }
                }
            }

            image.Store(filename);
        }
    }
}