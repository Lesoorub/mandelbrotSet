using System;
using System.Windows.Forms;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Newtonsoft.Json.Linq;

namespace MandelbrotSet
{ }
class Program
{
    public static ComplexM C = new ComplexM(0m, 0);
    public static int itercount = 10;
    public static decimal scalefactor = 620;
    public static int threads = 4;
    public static int qWidth = 7680;
    public static int qHeight = 7680;
    public static WinState state = WinState.FullWindow;
    public static int ScreenIndex = 0;

    static Font font = new Font("6622.ttf");
    static bool Stop = false;
    static Screen scr = Screen.AllScreens[ScreenIndex];

    [STAThread]
    static void Main(string[] args)
    {
        InitGrayTable();
        Load();
        VideoMode mode = new VideoMode((uint)scr.Bounds.Width, (uint)scr.Bounds.Height);
        RenderWindow app = null;
        if (state == WinState.FullWindow)
        {
            app = new RenderWindow(mode, "", Styles.None);
            app.Position = new Vector2i(scr.Bounds.Left, scr.Bounds.Top);
        } else
        if (state == WinState.Fullscreen)
        {
            app = new RenderWindow(mode, "", Styles.Fullscreen);
        } else
        if (state == WinState.Windowed)
        {
            mode.Width = mode.Width / 2;
            mode.Height = mode.Height / 2;
            app = new RenderWindow(mode, "");
        }
        app.SetFramerateLimit(60);
        app.Closed += (e, s) => app.Close();
        Image img = new Image(mode.Width, mode.Height);
        app.MouseWheelScrolled += (e, s) =>
        {
            scalefactor += scalefactor / 10m * (decimal)s.Delta;
            Save();
        };
        app.MouseButtonPressed += (e, s) =>
        {
            if (s.Button == Mouse.Button.Left)
            {
                Vector2f pos = app.MapPixelToCoords(new Vector2i(s.X - (int)(mode.Width / 2), s.Y - (int)(mode.Height / 2)));
                C = new ComplexM(C.Real + (decimal)pos.X / scalefactor, C.Imaginary + (decimal)pos.Y / scalefactor);
                Save();
            }
        };
        app.KeyPressed += (e, s) =>
        {
            decimal step = 5;
            if (s.Code == Keyboard.Key.Q)
                scalefactor += scalefactor / 10;
            if (s.Code == Keyboard.Key.E)
                scalefactor -= scalefactor / 10;
            if (s.Code == Keyboard.Key.D)
                C = new ComplexM(C.Real + step / scalefactor, C.Imaginary);
            if (s.Code == Keyboard.Key.A)
                C = new ComplexM(C.Real - step / scalefactor, C.Imaginary);
            if (s.Code == Keyboard.Key.S)
                C = new ComplexM(C.Real, C.Imaginary + step / scalefactor);
            if (s.Code == Keyboard.Key.W)
                C = new ComplexM(C.Real, C.Imaginary - step / scalefactor);
            if (s.Code == Keyboard.Key.Z)
                itercount *= 2;
            if (s.Code == Keyboard.Key.X)
                itercount /= 2;
            if (s.Code == Keyboard.Key.C)
            {
                if (!s.Shift)
                    img.SaveToFile("imgs/" + RandStr() + ".png");
                else if (s.Control)
                    QualiedImage(256, 256);
                else
                    QualiedImage(qWidth, qHeight);
            }
            if (s.Code == Keyboard.Key.R)
                Colorizeid++;
            if (s.Code == Keyboard.Key.F)
                Colorizeid--;
            if (s.Code == Keyboard.Key.Escape)
                app.Close();
            if (s.Code == Keyboard.Key.P)
                OpenSettings();
            if (Colorizeid > 5)
                Colorizeid = 0;
            if (Colorizeid < 0)
                Colorizeid = 5;
            if (itercount <= 0)
                itercount = 1;
            if (itercount > 2560)
                itercount = 2560;
            Save();
        };
        Text C_text = new Text("", font);
        C_text.CharacterSize = 8;
        int len = (int)((double)mode.Width / (double)threads);
        for (int k = 0; k < threads; k++)
        {
            int k1 = k;
            Task.Factory.StartNew(() => {
                while (app.IsOpen)
                {
                    if (Stop)
                        System.Threading.Thread.Sleep(1000);
                    else
                    {
                        Update(ref img, -1.5m, -1.5m,
                           (int)(k1 * len),
                           len, scalefactor);
                    }
                }
            });
        }
        while (app.IsOpen)
        {
            app.Clear();
            app.DispatchEvents();
            using (Texture tex = new Texture(img))
            using (Sprite spr = new Sprite(tex))
            {
                app.Draw(spr);
            }
            C_text.DisplayedString = 
                $"C: {C}\n" +
                $"Scale: {scalefactor}\n" +
                $"Iters: {itercount}\n" +
                $"Penid: {Colorizeid}";

            app.Draw(C_text);
            app.Display();
        }
    }
    static Random rnd = new Random();
    static void Update(ref Image img, decimal mx, decimal my, int startscr, int scrlen, decimal s = 20)
    {
        decimal xoffset = img.Size.X / 2 / s;
        decimal yoffset = img.Size.Y / 2 / s;
        uint t = (uint)(startscr + scrlen);
        uint height = img.Size.Y;
        for (uint x = (uint)startscr; x < t; x++)
            for (uint y = 0; y < height; y++)
            {
                int count = FastisInMandelbrotSet(Complex.Zero,
                    new ComplexM((decimal)x / s - xoffset, (decimal)y / s - yoffset) + C, itercount);
                img.SetPixel(x, y,
                    count == itercount ?
                    Color.Black : Colorize(count));
            }
    }
    public static void QualiedImage(int width, int height)
    {
        if (Stop == true) return;
        Stop = true;
        int
            curwork = 0,
            maxwork = width * height;
        Image img = new Image((uint)width, (uint)height);
        Task.Factory.StartNew(() => 
        {
            string filename = $"imgs/qualied_{RandStr(10)}({width},{height})." + (width == 256 && height == 256 ? "ico" : "png");
            int lastwork = -1;
            while (Stop)
            {
                if (MandelbrotSet.settingsform.bar != null)
                {
                    MandelbrotSet.settingsform.QualityProgress = ((float)curwork / (float)maxwork);
                }
                Console.WriteLine(((double)curwork / (double)maxwork * 100d) + "%");
                //img.SaveToFile(filename);
                System.Threading.Thread.Sleep(1000);
                if (curwork == maxwork || curwork == lastwork)
                {
                    img.SaveToFile(filename);
                    Console.WriteLine("\aComplete");
                    MandelbrotSet.settingsform.QualityProgress = 0;
                    Stop = false;
                }
                else lastwork = curwork;
            }
        });
        Console.WriteLine("Start");
        ComplexM C = new ComplexM(Program.C.Real, Program.C.Imaginary);
        decimal s = Program.scalefactor * ((decimal)width / (decimal)scr.Bounds.Width);
        int itercount = Program.itercount;
        int len = (int)((double)width / (double)threads);
        for (int k = 0; k < threads; k++)
        {
            int k1 = k;
            int scrlen = len;
            int startscr = (int)(k1 * len);
            Task.Factory.StartNew(() =>
            {
                decimal xoffset = img.Size.X / 2 / s;
                decimal yoffset = img.Size.Y / 2 / s;
                uint t = (uint)(startscr + scrlen);
                for (uint x = (uint)startscr; x < t; x++)
                    for (uint y = 0; y < height; y++)
                    {
                        decimal dx = (decimal)x / s - xoffset;
                        decimal dy = (decimal)y / s - yoffset;

                        int count = FastisInMandelbrotSet(Complex.Zero,
                            new ComplexM(dx, dy) + C, itercount);
                        img.SetPixel(x, y,
                            count == itercount ?
                            Color.Black : Colorize(count));
                        curwork++;
                    }
            });
        }
    }
    static ComplexM F(ComplexM z, ComplexM C)
    {
        return z*z + C;
    }
    static int FastisInMandelbrotSet(ComplexM z, ComplexM c, int iter)
    {
        //double x = c.Real;
        //double y = c.Imaginary;
        //double t = x - 0.25;
        //double p = Math.Sqrt(t * t + y * y);
        //double o = Math.Atan2(y, t);
        //double pc = 0.5 - Math.Cos(o) / 2;
        //if (p <= pc)
        //    return iter;
        return isInMandelbrotSet(z, c, iter);
    }
    static int isInMandelbrotSet(ComplexM z, ComplexM c, int iter)
    {
        for (int k = 0; k < iter; k++)
        {
            if (z.Real * z.Real < iter)
                z = F(z, c);
            else
                return k;
        }
        return iter;
    }
    static int Colorizeid = 0;
    static Color Colorize(int c)
    {
        switch (Colorizeid)
        {
            case 0:
                return GrayTable[c % 256];
            case 1:
                return ColorFromHSV((((double)c / 640) * 360 + 180) % 360, 1, 1);//фивалетовый
            case 2:
                return ColorFromHSV((((double)c / 640 / 2) * 360) % 360, 1, 1);//wtf?
            case 3:
                return ColorFromHSV(((double)c / itercount / 1) * 360, 1, 1);//лавовый
            case 4:
                return new Color(255, (byte)((9 * c) % 255), (byte)((9 * c) % 255));
        }
        return Color.White;
    }
    static Color[] GrayTable = new Color[256];
    static void InitGrayTable()
    {
        for (int k = 0; k < 256; k++)
            GrayTable[255 - k] = new Color((byte)k, (byte)k, (byte)k);
    }

    static void OpenSettings()
    {
        if (Application.OpenForms.Count != 0) return;
        MandelbrotSet.settingsform form = new MandelbrotSet.settingsform();
        Application.Run(form);
    }

    public static Color ColorFromHSV(double hue, double saturation, double value)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        int v = Convert.ToInt32(value);
        int p = Convert.ToInt32(value * (1 - saturation));
        int q = Convert.ToInt32(value * (1 - f * saturation));
        int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        if (hi == 0)
            return new Color((byte)v, (byte)t, (byte)p);
        else if (hi == 1)
            return new Color((byte)q, (byte)v, (byte)p);
        else if (hi == 2)
            return new Color((byte)p, (byte)v, (byte)t);
        else if (hi == 3)    
            return new Color((byte)p, (byte)q, (byte)v);
        else if (hi == 4)    
            return new Color((byte)t, (byte)p, (byte)v);
        else                 
            return new Color((byte)v, (byte)p, (byte)q);
    }
    static string save_path = "last.json";
    static void Save()
    {
        JObject j = new JObject();
        j.Add("C", JArray.Parse($"[{C.Real.ToString().Replace(',', '.')},{C.Imaginary.ToString().Replace(',', '.')}]"));
        j.Add("s", scalefactor.ToString().Replace(',', '.'));
        j.Add("iter", itercount);
        j.Add("Colorizeid", Colorizeid);
        j.Add("threads", threads);
        j.Add("qWidth", qWidth);
        j.Add("qHeight", qHeight);
        j.Add("state", (int)state);
        j.Add("ScreenIndex", ScreenIndex);
        System.IO.File.WriteAllText(save_path, j.ToString());
    }
    static void Load()
    {
        if (System.IO.File.Exists(save_path))
        {
            JObject j = JObject.Parse(System.IO.File.ReadAllText(save_path));
            if (j["C"] != null)
                C = new Complex((double)j["C"][0], (double)j["C"][1]);
            if (j["s"] != null)
                scalefactor = (decimal)j["s"];
            if (j["iter"] != null)
                itercount = (int)j["iter"];
            if (j["Colorizeid"] != null)
                Colorizeid = (int)j["Colorizeid"];
            if (j["threads"] != null)
                threads = (int)j["threads"];
            if (j["qWidth"] != null)
                qWidth = (int)j["qWidth"];
            if (j["qHeight"] != null)
                qHeight = (int)j["qHeight"];
            if (j["state"] != null)
                state = (WinState)(int)j["state"];
            if (j["ScreenIndex"] != null)
            {
                ScreenIndex = (int)j["ScreenIndex"];
                scr = Screen.AllScreens[ScreenIndex];
            }
        }
    }
    static string allowedchars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
    static string RandStr(int len = 32)
    {
        string t = "";
        for (int k = 0; k < len; k++)
            t += allowedchars[rnd.Next(0, allowedchars.Length)];
        return t;
    }
}
public enum WinState
{
    Windowed = 0, Fullscreen, FullWindow
}
public class ComplexM
{
    public decimal Real;
    public decimal Imaginary;

    static int n = 1024;

    public ComplexM(double real, double imag)
    {
        Real = (decimal)real;// new BigFloat(real, n);
        Imaginary = (decimal)imag;// new BigFloat(imag, n);
    }
    public ComplexM(decimal real, decimal imag)
    {
        Real = real;// new BigFloat(real, n);
        Imaginary = imag;// new BigFloat(imag, n);
    }

    public static ComplexM operator +(ComplexM left, ComplexM right)
    {
        return new ComplexM(left.Real + right.Real, left.Imaginary + right.Imaginary);
    }
    public static ComplexM operator -(ComplexM left, ComplexM right)
    {
        return new ComplexM(left.Real - right.Real, left.Imaginary - right.Imaginary);
    }

    public static ComplexM operator *(ComplexM left, ComplexM right)
    {
        return new ComplexM(
            left.Real * right.Real - left.Imaginary * right.Imaginary,
            left.Real * right.Imaginary + left.Imaginary * right.Real);
    }

    public static implicit operator ComplexM(Complex a)
    {
        return new ComplexM(a.Real, a.Imaginary);
    }
}