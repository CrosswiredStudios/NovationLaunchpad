using System;
using NovationLaunchpad.Interfaces;
using NovationLaunchpad.Models.Launchpads;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System.IO;

namespace NovationLaunchpad.Models.Effects.Piskel
{
    public class PiskelChunk
    {
        public string Base64Png { get; }
        public byte[] Data { get; }
        public JArray Layout { get; }

        public PiskelChunk(string data)
        {
            var chunkData = JObject.Parse(data);

            Base64Png = chunkData["base64PNG"].ToString();
            Data = Convert.FromBase64String(Base64Png.Split(',')[1]);
            Layout = chunkData["layout"] as JArray;
        }
    }

    public class PiskelLayer
    {
        public PiskelChunk[] Chunks { get; }
        public List<Color[,]> Frames { get; private set; }
        public int FrameCount { get; }
        public string Name { get; }
        public int Opacity { get; }

        public PiskelLayer(string data)
        {

            var layerData = JObject.Parse(data);

            FrameCount = (int)layerData["frameCount"];
            Name = layerData["name"].ToString();
            Opacity = (int)layerData["opacity"];

            var chunksData = layerData["chunks"] as JArray;

            Chunks = new PiskelChunk[chunksData.Count];
            for (var index = 0; index < chunksData.Count; index++)
            {
                Chunks[index] = new PiskelChunk(chunksData[index].ToString());
            }

            var pngData = Chunks.First().Data;
            var bitmap = SKBitmap.Decode(pngData);
            var frameWidth = bitmap.Width / FrameCount;
            Frames = new List<Color[,]>();
            for (var frameIndex = 0; frameIndex < FrameCount; frameIndex++)
            {
                var frame = new Color[frameWidth, bitmap.Height];
                var xOffset = frameIndex * frameWidth;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < frameWidth; x++)
                    {
                        var pixel = bitmap.Pixels[bitmap.Width * y + (x + xOffset)];
                        frame[x, 7 - y] = Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
                    }
                }
                Frames.Add(frame);
            }
        }
    }

    public class PiskelFile
    {
        SKBitmap bitmap;

        public List<Color[,]> Animation { get; }
        public string Description { get; }
        public int Fps { get; }
        public int Height { get; }
        public PiskelLayer[] Layers { get; }
        public int ModelVersion { get; }
        public string Name { get; }
        public int Width { get; }

        public PiskelFile(string jsonString)
        {
            Animation = new List<Color[,]>();

            var jContents = JObject.Parse(jsonString);

            ModelVersion = (int)jContents["modelVersion"];

            var piskelData = jContents["piskel"];

            Description = piskelData["description"].ToString();
            Fps = (int)piskelData["fps"];
            Height = (int)piskelData["height"];
            Name = piskelData["name"].ToString();
            Width = (int)piskelData["width"];

            var layersData = piskelData["layers"] as JArray;

            Layers = new PiskelLayer[layersData.Count];
            for (var index = 0; index < layersData.Count; index++)
            {
                Layers[index] = new PiskelLayer(layersData[index].ToString());
            }

            var pngData = Layers.First().Chunks.First().Data;
            bitmap = SKBitmap.Decode(pngData);

            var totalFrames = Layers.First().FrameCount;
            var frameWidth = bitmap.Width / totalFrames;

            for (var frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                var frame = new Color[frameWidth, bitmap.Height];
                var xOffset = frameIndex * frameWidth;
                for (var y = 0; y < bitmap.Height; y++)
                {
                    for (var x = 0; x < frameWidth; x++)
                    {
                        var pixel = bitmap.Pixels[bitmap.Width * y + (x + xOffset)];
                        frame[x, 7 - y] = Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
                    }
                }
                Animation.Add(frame);
            }

            Debug.WriteLine($"Successfully loaded piskel file for {Name}");
        }
    }

    public delegate void OnCompleteEvent();

    public class PiskelEffect : LaunchpadEffect
    {
        int currentFrameIndex;
        SKBitmap bitmap;
        string _filePath;
        List<Color[,]> frames;
        bool isFinished;
        bool isInitiated;
        LaunchpadMk2 launchpad;
        bool loop;
        PiskelFile piskelFile;

        event OnCompleteEvent whenComplete;

        public override string Name => "Piskel";

        public PiskelEffect(string filePath, bool loop)
        {
            frames = new List<Color[,]>();
            isFinished = false;
            _filePath = filePath;
            this.loop = loop;
        }

        void DrawFrame(Color[,] frame)
        {
            var totalFrames = piskelFile.Layers.First().FrameCount;
            var frameWidth = bitmap.Width / totalFrames;

            for (var y = 0; y < frameWidth; y++)
                for (var x = 0; x < frameWidth; x++)
                {
                    launchpad.GridBuffer[x, y] = frame[x, y];
                }
            launchpad.FlushGridBuffer();
        }

        public override void Initiate(ILaunchpad launchpad)
        {
            this.launchpad = launchpad as LaunchpadMk2;
            try
            {
                using (var fileStream = File.OpenText(_filePath))
                {
                    var contents = fileStream.ReadToEnd();
                    piskelFile = new PiskelFile(contents);
                }

                var pngData = piskelFile.Layers.First().Chunks.First().Data;
                bitmap = SKBitmap.Decode(pngData);

                var totalFrames = piskelFile.Layers.First().FrameCount;
                var frameWidth = bitmap.Width / totalFrames;

                for (var frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                {
                    var frame = new Color[frameWidth, bitmap.Height];
                    var xOffset = frameIndex * frameWidth;
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        for (var x = 0; x < frameWidth; x++)
                        {
                            var pixel = bitmap.Pixels[bitmap.Width * y + (x + xOffset)];
                            frame[x, y] = Color.FromArgb(pixel.Alpha, pixel.Red, pixel.Green, pixel.Blue);
                        }
                    }
                    frames.Add(frame);
                }


                isInitiated = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public override void Terminate()
        {

        }

        public override void Update()
        {
            if (!isInitiated || isFinished) return;
            currentFrameIndex++;
            if (currentFrameIndex >= piskelFile.Layers.First().FrameCount)
            {
                if (loop)
                    currentFrameIndex = 0;
                else
                {
                    isFinished = true;
                    whenComplete?.Invoke();
                }
            }

            DrawFrame(frames[currentFrameIndex]);
        }
    }
}
