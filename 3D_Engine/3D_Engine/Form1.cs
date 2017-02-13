using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Engine
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        Mesh test;
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            
            int fps = (int)Math.Round(Utility.renderMeshToGraphics(test, camPos, camDir, g));
            label1.Invoke((MethodInvoker)(() => label1.Text = "FPS: " + fps));
        }
        Vector3 camPos = new Vector3(0, 0, 0);
        Vector3 camDir = new Vector3(0, 0, 0);
        Thread t;
        private void rotateLoop()
        {
            while (true)
            {
                test.rotation.y += 1f;
                //test.rotation.x += 1f;
                Thread.Sleep(50);
                try
                {
                    //this.CreateGraphics().Clear(Color.White);
                    this.InvokePaint(this, new PaintEventArgs(this.CreateGraphics(), new Rectangle()));
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                    t.Abort();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*test = new Mesh("cube");
            test.faces = new Face[] {
                new Face(new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(-1, -1, -1)),
                new Face(new Vector3(1, 1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1)),
                new Face(new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1)),
                new Face(new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1)),
                new Face(new Vector3(1, 1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1)),
                new Face(new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1)),
                new Face(new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(-1, -1, 1)),
                new Face(new Vector3(1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1)),
                new Face(new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1)),
                new Face(new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1)),
                new Face(new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1)),
                new Face(new Vector3(-1, -1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, 1))
            };
            test.position = new Vector3(0, 0, 3);
            t = new Thread(new ThreadStart(rotateLoop));
            t.Start();*/
            loadThing();
        }
        private async Task loadThing()
        {
            test = (await Utility.LoadJSONFileAsync("D:\\monkey.babylon"))[0];
            t = new Thread(new ThreadStart(rotateLoop));
            t.Start();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            camPos.x = (float)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            camPos.y = (float)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            camPos.z = (float)numericUpDown3.Value;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            camDir.x = (float)numericUpDown6.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            camDir.y = (float)numericUpDown5.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            camDir.z = (float)numericUpDown4.Value;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            Utility.fov = (float)numericUpDown7.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            Utility.width = (float)numericUpDown8.Value;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            Utility.height = (float)numericUpDown9.Value;
        }
    }

    
}
