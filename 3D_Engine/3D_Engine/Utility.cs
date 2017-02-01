using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Engine
{
    static class Utility
    {
        public static Vector2 mapPointTo2DFromCamera(Vector3 point, float fov, Vector2 screenSize)
        {
            float uh = (float)(1 / Math.Tan(Math.PI * (fov / 2) / 180));
            float asp = screenSize.x / screenSize.y;
            float uw = uh / asp;

            float far = 100;
            float near = 1;
            Matrix projectionMatrix = new Matrix();
            projectionMatrix.matrix = new float[][] {
                new float[] { uw, 0, 0, 0 },
                new float[] { 0, uh, 0, 0},
                new float[] { 0, 0, (far / (far - near)), -1 },
                new float[] { 0, 0, (far * near / (far - near)), 0}
            };


             projectionMatrix.input = new float[]
            {
                point.x,
                point.y,
                point.z,
                1f
            };

            float x = projectionMatrix.output[0] / projectionMatrix.output[3];
            float y = projectionMatrix.output[1] / projectionMatrix.output[3];
            float z = projectionMatrix.output[2] / projectionMatrix.output[3];
            if (z <= -1 || z >= 1) return null;
            return new Vector2(x * (screenSize.x / 2) + (screenSize.x / 2), -y * (screenSize.y / 2) + (screenSize.y / 2));
        }

        public static Vector3 transformPointAroundCamera(Vector3 point, Vector3 camera, Vector3 cameraDirection)
        {
            point -= camera;                                                               // The camera needs to be at the origin
            point = applyRotationToPoint(point, new Vector3(0, 0, 0) - cameraDirection);

            // Everything is now relative to 0, 0, 0 facing directly towards Z.

            return point;
        }

        
        public static float cos(float angle){
            return (float)Math.Cos(angle * (Math.PI / 180f));
        }
        
        public static float sin(float angle){
            return (float)Math.Sin(angle * (Math.PI / 180f));
        }
        
        public static Vector3 applyRotationToPoint(Vector3 localPosition, Vector3 rotation)
        {
            float x = rotation.x;
            float y = rotation.y;
            float z = rotation.z;
            Matrix rotationMatrix = new Matrix();
            rotationMatrix.matrix = new float[][]{
                new float[]{ cos(y) * cos(z), (cos(z) * sin(x) * sin(y)) - (cos(x) * sin(z)), (cos(x) * cos(z) * sin(y)) + (sin(x) * sin(y)), 0 },
                new float[]{ cos(y) * sin(z), (cos(x) * cos(z)) + (sin(x) * sin(y) * sin(z)), -(cos(z) * sin(x)) + (cos(x) * sin(y) * sin(z)), 0 },
                new float[]{ -sin(y), (cos(y) * sin(x)), (cos(x) * cos(y)), 0 },
                new float[]{ 0, 0, 0, 1 }
            };
            
            rotationMatrix.input = new float[] { localPosition.x, localPosition.y, localPosition.z, 1f };
            
            // BIIIGGG matrix does a full rotation.
            
            float[] matrixOut = rotationMatrix.output;
            return new Vector3(matrixOut[0], matrixOut[1], matrixOut[2]);
        }

        public static float fov = 100;
        public static float height = 100;
        public static float width = 100;

        public static Graphics renderMeshToGraphics(Mesh m, Vector3 camPos, Vector3 camDir, Graphics g)
        {
            Face[] _faces = new Face[m.faces.Length];
            int i = 0;
            foreach (Face f in m.faces)
            {
                Face face = new Face();
                face.a = Utility.applyRotationToPoint(f.a, m.rotation);
                face.b = Utility.applyRotationToPoint(f.b, m.rotation);
                face.c = Utility.applyRotationToPoint(f.c, m.rotation);
                _faces[i] = face;
                i++;
            }

            // Local rotation applied, now apply camera rotation and positioning.

            Face[] faces = new Face[_faces.Length];
            i = 0;
            foreach (Face f in _faces)
            {
                Face face = new Face();
                face.a = transformPointAroundCamera(f.a + m.position, camPos, camDir);
                face.b = transformPointAroundCamera(f.b + m.position, camPos, camDir);
                face.c = transformPointAroundCamera(f.c + m.position, camPos, camDir);
                faces[i] = face;
                i++;
            }

            // Rotation applied around camera! Now simply drawing.

            foreach (Face f in faces)
            {
                float aThickness = (10 - magnitude(f.a)) - 5;
                float bThickness = (10 - magnitude(f.b)) - 5;
                float cThickness = (10 - magnitude(f.c)) - 5;
                Vector2 a = mapPointTo2DFromCamera(f.a, fov, new Vector2(width, height));
                Vector2 b = mapPointTo2DFromCamera(f.b, fov, new Vector2(width, height));
                Vector2 c = mapPointTo2DFromCamera(f.c, fov, new Vector2(width, height));

                try
                {
                    if (a != null && b != null) g.DrawLine(Pens.Black, a.x, a.y, b.x, b.y);
                    if (a != null && c != null) g.DrawLine(Pens.Black, a.x, a.y, c.x, c.y);
                    if (b != null && c != null) g.DrawLine(Pens.Black, b.x, b.y, c.x, c.y);
                    /*
                    if (a != null && b != null) drawLine(g, a, b, aThickness, bThickness);//(100 - f.a.z) / 30, (100 - f.b.z) / 30);
                    if (a != null && c != null) drawLine(g, a, c, aThickness, cThickness);//(100 - f.a.z) / 30, (100 - f.c.z) / 30);
                    if (b != null && c != null) drawLine(g, b, c, bThickness, cThickness);//(100 - f.b.z) / 30, (100 - f.c.z) / 30);*/
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception during rendering: " + e.ToString());
                }
            }

            // Done!

            return g;
        }

        private static void drawLine(Graphics g, Vector2 a, Vector2 b, float aWidth, float bWidth)
        {
            int diff = (int)aWidth - (int)bWidth;
            if (diff < 0) diff = -diff;
            diff /= 2;
            float minWidth = aWidth > bWidth ? bWidth : aWidth;
            for(int i = 0; i <= diff; i++)
            {
                Vector2 v = b - a;
                float xEach = v.x;
                float yEach = v.y;
                if (diff != 0)
                {
                    xEach = v.x / diff;
                    
                    yEach = v.y / diff;
                    
                }
                Pen p = new Pen(Color.Black, minWidth + (2 * i));
                Pen pen = new Pen(new SolidBrush(Color.Blue));
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Flat;
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Flat;
                pen.Width = minWidth + (2 * i);
                g.DrawLine(pen, a.x + (xEach * i), a.y + (yEach * i), a.x + xEach + (xEach * i), a.y + yEach + (yEach * i));
            }
        }

        private static float magnitude(Vector3 v)
        {
            float x = v.x * v.x;
            float y = v.y * v.y;
            float z = v.z * v.z;
            return (float)Math.Sqrt(x + y + z);
        }
    }
}
