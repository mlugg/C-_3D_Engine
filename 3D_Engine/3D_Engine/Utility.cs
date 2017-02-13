using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static float renderMeshToGraphics(Mesh m, Vector3 camPos, Vector3 camDir, Graphics g)
        {
            Stopwatch timer = Stopwatch.StartNew();
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
            int w = (int)Math.Round(width);
            int h = (int)Math.Round(height);
            int[][] buffer = new int[w][];
            for(int j = 0; j < buffer.Length; j++)
            {
                buffer[j] = new int[h];
                for(int k = 0; k < h; k++)
                {
                    buffer[j][k] = 0;
                }
            }

            // Rotation applied around camera! Now simply drawing.
            bmp.Dispose();
            bmp = new DirectBitmap((int)Math.Round(width), (int)Math.Round(height));
            foreach (Face f in faces)
            {
                Vector2 a = mapPointTo2DFromCamera(f.a, fov, new Vector2(width, height));
                Vector2 b = mapPointTo2DFromCamera(f.b, fov, new Vector2(width, height));
                Vector2 c = mapPointTo2DFromCamera(f.c, fov, new Vector2(width, height));

                try
                {
                    if (a != null && b != null) drawLine(bmp, a, b);
                    if (a != null && c != null) drawLine(bmp, a, c);
                    if (b != null && c != null) drawLine(bmp, b, c);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception during rendering: " + e.ToString());
                }
            }

            // Done!
            
            g.Clear(Color.White);

            g.DrawImage(bmp.Bitmap, new Point(0, 0));
            timer.Stop();
            float ms = timer.ElapsedMilliseconds;
            float fps = 1000 / ms;
            return fps;
        }
        static DirectBitmap bmp = new DirectBitmap((int)Math.Round(width), (int)Math.Round(height));

        // Credit to some genius on the internet
        public static void drawLine(DirectBitmap bmp, Vector2 a, Vector2 b)
        {
            if (a.x >= width | a.x < 0 | b.x >= width | b.x < 0 | a.y >= height | a.y < 0 | b.y >= height | b.y < 0) return;
            try
            {
                int x = (int)Math.Round(a.x);
                int y = (int)Math.Round(a.y);
                int x2 = (int)Math.Round(b.x);
                int y2 = (int)Math.Round(b.y);
                int w = x2 - x;
                int h = y2 - y;
                int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
                if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
                if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
                if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
                int longest = Math.Abs(w);
                int shortest = Math.Abs(h);
                if (!(longest > shortest))
                {
                    longest = Math.Abs(h);
                    shortest = Math.Abs(w);
                    if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                    dx2 = 0;
                }
                int numerator = longest >> 1;
                for (int i = 0; i <= longest; i++)
                {
                    if (bmp.Width > x && bmp.Height > y) bmp.Bits[x + (y * bmp.Width)] = Color.Black.ToArgb();
                    else return;
                    numerator += shortest;
                    if (!(numerator < longest))
                    {
                        numerator -= longest;
                        x += dx1;
                        y += dy1;
                    }
                    else
                    {
                        x += dx2;
                        y += dy2;
                    }
                }
            }
            catch
            {
            }
        }

        private static float magnitude(Vector3 v)
        {
            float x = v.x * v.x;
            float y = v.y * v.y;
            float z = v.z * v.z;
            return (float)Math.Sqrt(x + y + z);
        }


        public static async Task<Mesh[]> LoadJSONFileAsync(string fileName)
        {
            var meshes = new List<Mesh>();
            byte[] result;

            using (FileStream SourceStream = File.Open(fileName, FileMode.Open))
            {
                result = new byte[SourceStream.Length];
                await SourceStream.ReadAsync(result, 0, (int)SourceStream.Length);
            }
            string text = System.IO.File.ReadAllText(fileName);
            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(text);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value);

                Vector3[] vertices = new Vector3[verticesCount];

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    vertices[index] = new Vector3(x, y, z);
                }
                mesh.faces = new Face[facesCount];
                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.faces[index] = new Face(vertices[a], vertices[b], vertices[c]);
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }
    }

    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}
